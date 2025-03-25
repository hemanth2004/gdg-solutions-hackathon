from google.cloud import texttospeech
import os
import base64
import re

os.environ['GOOGLE_APPLICATION_CREDENTIALS'] = 'tts-credentials.json'

class TTSClient:
    """Client for Google Text-to-Speech API."""
    
    def __init__(self):
        self.client = texttospeech.TextToSpeechClient()

    def text_to_speech(
        self, 
        text: str, 
        output_file: str = "output.mp3", 
        lang: str = "en-IN", 
        return_encoded: bool = False
    ) -> None:
        """Synthesize text to speech and save to file."""
        
        text = re.sub(r"[*_~]", "", text)

        synthesis_input = texttospeech.SynthesisInput(text=text)

        voice = texttospeech.VoiceSelectionParams(
            language_code=lang,
            ssml_gender=texttospeech.SsmlVoiceGender.NEUTRAL,
            name="en-IN-Standard-D"
        )

        audio_config = texttospeech.AudioConfig(
            audio_encoding=texttospeech.AudioEncoding.MP3,
            speaking_rate=1.25,
            pitch=1
        )

        response = self.client.synthesize_speech(
            input=synthesis_input, voice=voice, audio_config=audio_config
        )

        if return_encoded:
            return base64.b64encode(response.audio_content).decode()

        with open(output_file, "wb") as out:
            out.write(response.audio_content)
            

