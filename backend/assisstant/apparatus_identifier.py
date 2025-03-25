from typing import List
import enum

from google import genai
from google.genai import types

import PIL.Image

class ApparatusIdentifier:
    def __init__(self):
        self.client = genai.Client()
        self.MODEL_ID = "gemini-2.0-flash"

    def set_appratus_enum(self, apparatus_enum: enum.Enum):
        self.apparatus_enum = apparatus_enum

    def set_apparatus_from_list(self, apparatus_list: List[str]):
        apparatus_list.append("unknown")
        self.set_appratus_enum(enum.Enum("Apparatus", {apparatus.upper(): apparatus.lower() for apparatus in apparatus_list}))

    def identify_apparatus(self, image_path: str) -> str:
        image = PIL.Image.open(image_path)

        # Generate response
        response = self.client.models.generate_content(
            model = self.MODEL_ID,
            contents=["Identify the apparatus in the image.", image],
            config={
                "response_mime_type": "text/x.enum",
                "response_schema": self.apparatus_enum
            }
        )

        # Extract and return the answer
        result = response.text.strip()
        if result in [apparatus.value for apparatus in self.apparatus_enum]:
            return result
        return "Unknown"

if __name__ == "__main__":
    apparatus_list = ["ammeter", "voltmeter", "battery", "rheostat", "plug_key"]

    appratus_identifier = ApparatusIdentifier()
    appratus_identifier.set_apparatus_from_list(apparatus_list)
    
    import os
    for image_path in os.listdir("test_images/apparatus"):
        image_path = os.path.join("test_images/apparatus", image_path)
        print(f"Image Path: {image_path}\tIdentified apparatus: {appratus_identifier.identify_apparatus(image_path)}")

# FROM UNITY: apparatus_list, frame
# TO UNITY: name: str
