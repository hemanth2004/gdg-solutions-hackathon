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
import json

class ARExperiment(TypedDict):
    name: str                               # Experiment name
    visualizations: List[Tuple[str, str]]   # List of available visualizations
    apparatus_schema: str                   # Schema of the apparatus used in the experiment

class State(TypedDict):
    messages: Annotated[Sequence[BaseMessage], add_messages]    # List of messages in the conversation to use as context
    language: str                                               # Language of the conversation
    image_path: str                                             # Path to the image (local as of now)
    experiment: ARExperiment                                    # Current experiment

class ARAssisstant:
    def __init__(self):
        self.model = ChatGoogleGenerativeAI(model="gemini-2.0-flash-001", temperature=0.2)
        self.trimmer = trim_messages(
            max_tokens=2048,
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
                    """
            You are a virtual lab instructor for science experiments aligned with the CBSE syllabus.
            The student is conducting the {experiment_name} experiment.
            Respond in {language}. 
            If the user speaks in a different language, respond in the language the user spoke. Give priority to the language spoken by the user.
            
            Your job is to guide the student through the experiment and perform valid actions.

            ## RESPONSE FORMAT:
            Respond using a single <Response> ... </Response> block.

            - Embed instructional explanation text inside this block.
            - Insert <Action> ... </Action> tags inside it whenever an action must be performed.
            - Each action must follow the strict format:

            <Action>
                <SetField apparatusId="..." fieldToSet="..." value="..." />
            </Action>

            or

            <Action>
                <Visualization name="..." value="on|off" />
            </Action>

            **IMPORTANT**: Only set fields from the predefined schema given below. Do not invent new apparatus or fields.

            -----------------------------------
            ## APPARATUS SCHEMA
            -----------------------------------
            {apparatus_schema}
            -----------------------------------

            ## FIELD RULES:
            - For slider fields, use a value **within the min and max range**.
                Example: resistance = 100 (valid), resistance = 5000 (invalid)
            - For boolean fields, use either `true` or `false` (as lowercase strings).
                Example: closed = true
            - Do NOT attempt to set a field or apparatus not listed in the schema.
            - Do NOT guess min/max — always stay within defined bounds.
            - Do NOT set multiple fields in one <Action>. Use one per action.

            ## EXAMPLE OUTPUT:
            <Response>
            Let's start by turning on the voltage source.

            <Action>
                <SetField apparatusId="voltageSource" fieldToSet="on" value="true" />
            </Action>

            Now, we'll increase the voltage to 5 volts.

            <Action>
                <SetField apparatusId="voltageSource" fieldToSet="voltage" value="5" />
            </Action>

            Notice how the bulb starts glowing.

            <Action>
                <Visualization name="current_flow" value="on" />
            </Action>

            We'll turn off the visualization before moving on.

            <Action>
                <Visualization name="current_flow" value="off" />
            </Action>
            </Response>

            -----------------------------------

            ## BEHAVIOR GUIDELINES:
            - Always use available visualizations: {available_visualizations}
            - Always assume the student wants a full explanation and action to be performed.
            - Do not ask for confirmation before setting fields or changing visuals.
            - Automatically disable visuals after they're no longer needed.
            - Respect field types, ranges, and apparatus IDs strictly.

            Use this schema and formatting rules in all responses.

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
            image_base64, mime_type = encode_image(state["image_path"])

            # Add image as part of the message content
            trimmed_messages.append(
                HumanMessage(
                    content=[
                        {"type": "text", "text": "Here is the image."},
                        {
                            "type": "image_url",
                            "image_url": {"url": f"data:{mime_type};base64,{image_base64}"},
                        },
                    ]
                )
            )

        vis_as_strings = [f"{viz[0]} - {viz[1]}" for viz in state["experiment"]["visualizations"]]
        available_vis = ", ".join(vis_as_strings) if vis_as_strings else "none available"
        print("\033[94m" + str(available_vis) + "\033[0m")

        prompt = self.prompt_template.invoke(
            {
                "messages": trimmed_messages,
                "language": state["language"],
                "experiment_name": state["experiment"]["name"],
                "available_visualizations": available_vis,
                "apparatus_schema": state["experiment"]["apparatus_schema"]
            }
        )
        
        print("\033[93m" + f"Tokens in prompt: {self.model.get_num_tokens(str(prompt))}" + "\033[0m")
        print("\033[93m" + str(prompt) + "\033[0m")

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
    
        # Remove any incorrect closing tags (e.g., </name:off>)
        text = re.sub(r"</\w+:(on|off)>", r"<\1:off>", text)

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
        sequence_to_send = {
            "vis": [],
            "audio": [],
        }

        for item in execution_sequence:
            if isinstance(item, tuple):
                sequence_to_send["vis"].append(f"{item[0]} {item[1]}")
            else:
                if save_audio:
                    audio_counter += 1
                    audio_path = f"audio_store/output_{audio_counter}.mp3"
                    self.tts.text_to_speech(item, output_file=audio_path)
                
                b64_audio = self.tts.text_to_speech(item, return_encoded=True)
                sequence_to_send["audio"].append(b64_audio)

        # Convert sequence_to_send to JSON
        payload = json.dumps({"sequence": sequence_to_send})

        return payload
    
    def _parse_xml_response(self, response: str, save_audio=False) -> str:
        audio_counter = 0

        # Extract content between <Response>...</Response>
        match = re.search(r"<Response>(.*?)</Response>", response, re.DOTALL)
        if not match:
            return {"xml": response.strip()}  # fallback if invalid

        inner_content = match.group(1)

        # Split content into text and <Action> blocks
        segments = re.split(r"(<Action>.*?</Action>)", inner_content, flags=re.DOTALL)

        new_segments = []
        for seg in segments:
            seg = seg.strip()
            if not seg:
                continue
            if seg.startswith("<Action>"):
                new_segments.append(seg)
            else:
                # It's plain text — convert to audio
                if save_audio:
                    audio_counter += 1
                    audio_path = f"audio_store/output_{audio_counter}.mp3"
                    self.tts.text_to_speech(seg, output_file=audio_path)
                b64_audio = self.tts.text_to_speech(seg, return_encoded=True)
                new_segments.append(f"<Audio>{b64_audio}</Audio>")

        # Reconstruct wrapped response
        final_xml = "<Response>\n  " + "\n\n  ".join(new_segments) + "\n</Response>"

        return json.dumps({ "xml": final_xml })


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
        print("\033[92m" + response + "\033[0m")
        
        payload = self._parse_xml_response(response, save_audio=True)
        return payload
    
if __name__ == "__main__":
    assistant = ARAssisstant()
    ohms_law_exp = ARExperiment(name="Ohm's Law", 
                                visualizations=[
                                    ("voltage_gradient", "color graident of voltage along wires"), 
                                    ("electron_flow", "shows blue particles as electrons flowing along wires")
                                ],
                                apparatus_schema="""
                                {
                                    "resistorBox": {
                                        "resistance": { "type": "slider", "min": 10, "max": 1000 },
                                        "enabled": { "type": "boolean" }
                                    },
                                    "voltageSource": {
                                        "voltage": { "type": "slider", "min": 0, "max": 10 },
                                        "on": { "type": "boolean" }
                                    }
                                }""")
    
    assistant.set_current_experiment(ohms_law_exp)

    thread_id = "abc123"

    # a loop to interact with the assistant
    while True:
        query = input("query> ")
        if query == "q": break
        language = "English"
        image_path = "with_wires.png"
        print(assistant.invoke(thread_id, query, language, image_path))
        # print(response)

# Experiment JSON (generated in UNITY):
# { name, [(vis, description)] }

# FROM UNITY: thread_id, text, language, image_path, Experiment Data
# then convert ExperimentData to ARExperiment Typed Dict
# call invoke

# TO UNITY: sequence { audio: [b64], vis: ["name state"]}

# thread_id -> Experiment Data in a database for later