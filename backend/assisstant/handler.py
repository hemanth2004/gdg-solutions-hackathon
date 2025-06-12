from assisstant import ARAssisstant, ARExperiment
from storage_service import compose_request_apparatus_schema
import os

import base64
import tempfile
import os
from datetime import datetime

def handle_chat_request(request):
    # print("Image b64: ", request.base64Image)
    image_path = base64_to_image_path(request.base64Image, thread_id=request.sessionID)

    print("Composing schema")
    assisstant = ARAssisstant()
    exp = ARExperiment(
        name=request.experimentContext.name,
        visualizations=[(v.name, v.desc) for v in request.experimentContext.availableVisualizations],
        apparatus_schema=compose_request_apparatus_schema(
            request.experimentContext.instantiatedApparatus,
            request.experimentContext.requiredApparatus
        )
    )
    

    assisstant.set_current_experiment(exp)
    response = assisstant.invoke(
        thread_id=request.sessionID,
        query=request.prompt,
        language=request.language,
        image_path=image_path
    )
    return response

def base64_to_image_path(base64_image: str, thread_id: str = None):
    if not base64_image:  # If base64_image is empty
        return None
        
    # Create base directory if it doesn't exist
    base_dir = "test_images"
    if not os.path.exists(base_dir):
        os.makedirs(base_dir)
    
    # Create thread directory if thread_id is provided
    if thread_id:
        thread_dir = os.path.join(base_dir, thread_id)
        if not os.path.exists(thread_dir):
            os.makedirs(thread_dir)
    else:
        thread_dir = base_dir
    
    # Convert base64 to image
    image = base64.b64decode(base64_image)
    
    # Save image with timestamp as filename
    filename = f"image.png"
    file_path = os.path.join(thread_dir, filename)
    
    with open(file_path, 'wb') as f:
        f.write(image)
    
    return os.path.abspath(file_path)

