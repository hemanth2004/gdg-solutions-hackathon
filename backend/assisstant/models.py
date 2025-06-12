from pydantic import BaseModel
from typing import Dict, Any, List, Optional


class ApparatusInstance(BaseModel):
    name: str
    apparatusId: int

class Visualization(BaseModel):
    name: str
    desc: str

class ExperimentContext(BaseModel):
    schoolClass: str
    subject: str
    topic: str
    name: str
    theory: str
    procedure: str
    requiredApparatus: List[str]
    instantiatedApparatus: List[ApparatusInstance]
    availableVisualizations: List[Visualization]

class ChatRequest(BaseModel):
    sessionID: str
    prompt: str
    base64Image: Optional[str]
    experimentContext: ExperimentContext
    language: str

class ChatResponse(BaseModel):
    message: str
    data: Dict[str, Any]
    audio_base64: Optional[str] = None
    visualizations: Optional[List[Dict[str, str]]] = None