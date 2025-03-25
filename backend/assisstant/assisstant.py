from image_encoder import encode_image
import tts

from typing import Sequence, List, Union, Tuple
from typing_extensions import Annotated, TypedDict

import re

from langchain_google_genai import ChatGoogleGenerativeAI
from langchain_core.messages import HumanMessage
from langgraph.checkpoint.memory import MemorySaver
from langgraph.graph import START, StateGraph
from langchain_core.prompts import ChatPromptTemplate, MessagesPlaceholder
from langchain_core.messages import BaseMessage
from langgraph.graph.message import add_messages
from langchain_core.messages import trim_messages

class ARExperiment(TypedDict):
    name: str                   # Experiment name
    visualizations: List[str]   # List of available visualizations

class State(TypedDict):
    messages: Annotated[Sequence[BaseMessage], add_messages]    # List of messages in the conversation to use as context
    language: str                                               # Language of the conversation
    image_path: str                                             # Path to the image (local as of now)
    experiment: ARExperiment                                    # Current experiment

class ARAssisstant:
    def __init__(self):
        self.model = ChatGoogleGenerativeAI(model="gemini-2.0-flash-001")
        self.trimmer = trim_messages(
            max_tokens=1024,
            strategy="last",
            token_counter=self.model,
            include_system=True,
            allow_partial=False,
            start_on="human",
        )

        self.prompt_template = ChatPromptTemplate.from_messages(
            [
                (
                    "system",
                    """You are a virtual lab instructor for science experiments following the CBSE syllabus.
                    The student is conducting {experiment_name} experiment.
                    Answer all questions to the best of your ability in {language}.
                    
                    VISUALIZATION GUIDELINES:
                    - Available visualizations: {available_visualizations}
                    - Use the format <visualization_name:on/off> to control visualizations.
                    - Only enable visualizations when:
                    1. The student explicitly asks to see a visualization
                    2. A complex concept needs visual explanation
                    3. Demonstrating a specific aspect of the experiment would benefit from visual aid
                    - Always disable visualizations when:
                    1. Moving to a new topic or experiment step
                    2. The visual explanation is complete
                    3. The student asks to turn them off
                    4. At the end of your response
                    5. Turning on a different visualisation
                    
                    INSTRUCTIONAL APPROACH:
                    - Guide students step by step through the experiment
                    - Analyze what's visible in the student's current view
                    - Provide clear, age-appropriate explanations
                    - Help troubleshoot issues shown in images or described in text
                    - Only answer questions relevant to {experiment_name} or general science concepts
                    
                    You will receive images showing what the student currently sees. Use this information to provide contextual guidance.
                    """
                ),
                MessagesPlaceholder(variable_name="messages"),
            ]
        )

        self.workflow = StateGraph(state_schema=State)
        self._define_workflow()
        self.memory = MemorySaver() # TODO: use memory store or a databse (see docs)
        self.app = self.workflow.compile(checkpointer=self.memory)

        self.none_experiment = ARExperiment(experiment_name="CBSE Science Experiments", visualizations=[])
        self.current_experiment: ARExperiment = None
        
        self.tts = tts.TTSClient()


    def _define_workflow(self):
        self.workflow.add_edge(START, "model")
        self.workflow.add_node("model", self._call_model)


    def _call_model(self, state: State):
        trimmed_messages = self.trimmer.invoke(state["messages"])

        # Add image if it exists
        if "image_path" in state and state["image_path"]:
            image_url = encode_image(state["image_path"])

            # Add image as part of the message content
            trimmed_messages.append(
                HumanMessage(
                    content=[
                        {"type": "text", "text": "Here is the image."},
                        {"type": "image_url", "image_url": image_url},
                    ]
                )
            )

        prompt = self.prompt_template.invoke(
            {
                "messages": trimmed_messages,
                "language": state["language"],
                "experiment_name": state["experiment"]["name"],
                "available_visualizations": ", ".join(state["experiment"]["visualizations"]) if state["experiment"]["visualizations"] else "none available"
            }
        )
        response = self.model.invoke(prompt)
        return {"messages": [response]}

    def set_current_experiment(self, experiment: ARExperiment):
        self.current_experiment = experiment

    def remove_current_experiment(self):
        self.current_experiment = None

    def _split_response_by_visualizations(self, text: str) -> List[Union[str, Tuple[str, str]]]:
        """ Splits the text by visualization tags of the format <visualization_name:on/off>. """
        # Regular expression pattern to match <visualization_name:on/off>
        pattern = r"<(\w+):(on|off)>"

        result = []
        last_index = 0

        # Iterate over all matches
        for match in re.finditer(pattern, text):
            start, end = match.span()

            # Add preceding text
            if start > last_index:
                result.append(text[last_index:start].strip())

            # Add visualization tuple
            viz_name, viz_state = match.groups()
            result.append((viz_name, viz_state))

            # Update the last index
            last_index = end

        # Add any remaining text
        if last_index < len(text):
            result.append(text[last_index:].strip())

        return result

    def _execute_sequence(self, execution_sequence, save_audio=False):
        # Process the execution sequence

        audio_counter = 0
        sequence_to_send = []
        for item in execution_sequence:
            if isinstance(item, tuple):
                sequence_to_send.append(f"{item[0]} {item[1]}")
            else:
                if save_audio:
                    audio_counter += 1
                    audio_path = f"temp/output_{audio_counter}.mp3"
                    self.tts.text_to_speech(item, output_file=audio_path)
                
                b64_audio = self.tts.text_to_speech(item, return_encoded=True)
                sequence_to_send.append(b64_audio)

        # TODO: Send the sequence to UNITY and handle playing audio and toggling visualizations there

    def invoke(self, thread_id: str, query: str, language: str, image_path: str):
        config = {"configurable": {"thread_id": thread_id}}

        # Invoke the graph
        input_messages = [HumanMessage(query)]
        output = self.app.invoke(
            {
                "messages": input_messages,
                "language": language,
                "image_path": image_path,
                "experiment": self.current_experiment if self.current_experiment else self.none_experiment
            },
            config=config
        )
        response = output["messages"][-1].content
        
        # Generate execution sequence
        execution_sequence = self._split_response_by_visualizations(response)
        self._execute_sequence(execution_sequence, save_audio=False)
    

if __name__ == "__main__":
    assistant = ARAssisstant()
    ohms_law_exp = ARExperiment(name="Ohm's Law", visualizations=["voltage_gradient", "electron_flow"])
    
    assistant.set_current_experiment(ohms_law_exp)

    thread_id = "abc123"

    # a loop to interact with the assistant
    while True:
        query = input("query> ")
        if query == "q": break
        language = "English"
        image_path = "with_wires.png"
        assistant.invoke(thread_id, query, language, image_path)
        # print(response)