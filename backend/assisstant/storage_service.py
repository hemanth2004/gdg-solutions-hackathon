import os
import requests
from dotenv import load_dotenv
from typing import List
from models import ApparatusInstance
from urllib.parse import urlencode

# Load environment variables
load_dotenv()

SetField_Action_Guide  = """
For setting fields of apparatus, the apparatusId is the number that identifies the specific instance of the apparatus
The fieldToSet is the key of the field to set
The value is the value to set the field to
    For booleans, use either `true` or `false` (as lowercase strings).
    For Dropdowns, use the index of the dropdown option
    For buttons, just set the value to `true`
    For the rest, its pretty self explanatory
"""

def compose_request_apparatus_schema(apparatus_instances: List[ApparatusInstance], names: List[str]) -> str:
    schema = "Heres all the placed apparatuses:\n"
    for index, inst in enumerate(apparatus_instances):
        schema += f"{index+1}. {inst.name}, id:{inst.apparatusId}\n"

    schema += "\nSchema for all required apparatuses:\n"
    for index, name in enumerate(names):
        try:
            print("getting apparatus data for ", name)
            apparatus_data = get_apparatus_data(name)
            schema += f"{index+1}. {name}:\n{apparatus_data}\n\n"
        except Exception as e:
            print(f"Error getting apparatus data from storage service for {name}: {e}")
            continue
    
    return schema

def get_apparatus_data(name: str):
    base_url = os.getenv('ARLABS_STORAGE_SERVICE_URL')
    # Manually construct the URL with proper encoding
    encoded_params = urlencode({'name': name}, quote_via=lambda x, *args: x)
    url = f"{base_url}/api/apparatus?{encoded_params}"
    response = requests.get(url)
    response.raise_for_status()
    return response.json()

def get_experiment_data(name: str):
    base_url = os.getenv('ARLABS_STORAGE_SERVICE_URL')
    # Manually construct the URL with proper encoding
    encoded_params = urlencode({'name': name}, quote_via=lambda x, *args: x)
    url = f"{base_url}/api/experiments?{encoded_params}"
    response = requests.get(url)
    response.raise_for_status()
    return response.json()

def get_visualization_data(name: str):
    base_url = os.getenv('ARLABS_STORAGE_SERVICE_URL')
    # Manually construct the URL with proper encoding
    encoded_params = urlencode({'name': name}, quote_via=lambda x, *args: x)
    url = f"{base_url}/api/visualizations?{encoded_params}"
    response = requests.get(url)
    response.raise_for_status()
    return response.json()

def get_storage_service_health():
    base_url = os.getenv('ARLABS_STORAGE_SERVICE_URL')
    response = requests.get(f"{base_url}/health")
    response.raise_for_status()
    return response.json()

