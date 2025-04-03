<div align="center">
<h1 align="center">AR Labs</h1>
   <small>Google Developers Group Solutions Hackathon Entry</small>
</div>

> Problem Statement: Lack of Access to Quality Education in Underserved Communities

AR Labs is a mobile AR app that lets students perform virtual science experiments of class 11 & 12. 
1. The app provides an intuitive sandbox within which the student can play around with the apparatuses, materials and receive accurate readings. This encourages the student to explore beyond the experiment, in the true spirit of science. 
2. We also have an AI Lab assistant as a replacement for instructors in a real lab. This is because the AI Lab Assistant is 
   - Context-aware, aware of the not just the state of the experiment but also what the user is currently looking at.
   - It can respond to queries from the user on how to use the app or a question about the topic at hand
   - Can perform actions and hence exert its control over the experiment. It can toggle on/off visualizations or performan actions like placing an apparatus.

---

## Table of Contents
1. [Our Approach](#our-approach)
2. [Architecture](#architecture)
3. [Technologies We Used](#technologies-we-used)
4. [Key Features](#key-features)
5. [Installation and Setup Guide](#installation-and-setup-guide)
6. [Challenges We Faced](#challenges-we-faced)


---

## Our Approach
1. **Frontend (`/arlabs/`)**:
   - Mobile AR application running on the Unity engine
   - Works as the interface for users to perform virtual experiments
   - Voice-access to an AI Lab assistant that is context-aware, explains the experiment and can exert control over the experiment in the form of AI actions.

2. **Backend (`/backend/`)**:
   - Backend running on Python
   - Handles API requests using FastAPI
   - Orchestrates our LLM architecture using LangGraph, 
   - Uses Google Cloud Text-to-Speech features


---

## Architecture
The project follows a modular architecture:

![IMG-20250401-WA0009 1](https://gist.github.com/user-attachments/assets/08a04004-229b-4d3c-8f10-19558033e175)

---

## Technologies We Used
1. **Frontend (C#)**: 
   - The Unity Engine
   - ARCore, AR Kit and AR Foundation
   - Google Cloud for Speech-to-Text API

2. **Backend (Python)**:
   - FastAPI for API development.
   - LangGraph for AI workflow management.
   - Google Gemini as our primary LLM.
   - Google Cloud for Text-to-Speech API.
 
3. **Additional Tools**:
   - Blender for 3D model creation of experiment apparatuses.

---

## Key Features
1. **AR-Powered Experiments**:

   - Real-time object interaction and visualization.
   - AR-based science experiments for educational purposes.

2. **AI-Powered Assistance**:

   - Voice-controlled AI assistant for experiment guidance.
   - Answers questions related to experiments and concepts.

3. **Seamless API Integration**:

   - Real-time communication between frontend and backend.
   - Uses REST APIs for efficient data exchange.

4. **Multi-Language Support**:

   - AI assistant supports multiple languages for a broader reach
---


## Installation and Setup Guide

### Prerequisites
- Unity (6000.0.23f1 LTS) for the AR frontend.
- Python (3.8 or later) for the backend.

## Clone the repository:
   ```bash
   git clone https://github.com/hemanth2004/gdg-solutions-hackathon/
   cd /gdg-solutions-hackathon/arlabs
   ```
### Frontend Setup (Android)

1. Open the project in Unity Hub. 
2. Make sure the android build package is installed. Steps for Xcode and iOS builds are different.
3. Get your Speech-to-Text credentials from your Google cloud console and place them as `Assets/StreamingAssets/stt-credentials.json`
4. Open on Unity and use Ctrl+P to run in the editor.
5. Make sure Vulkan is disabled and API Level 24+ is chosen in the Player Settings.
6. Open File > Build Profiles and Build to build a .apk. 

### Backend Setup
1. Navigate to the backend folder:
```bash
cd /gdg-solutions-hackathon/backend/assisstant
```
2. Install dependencies
```bash
pip install -r requirements.txt
```
3. Setup the secret files:
   - 1. Create a .env file in the backend directory 
      ``` bash
      GOOGLE_GEMINI_API=''
      ```
   - 2. Get  a text-to-speech secret file and save it in the backend directory as **tts-credential.json** in `backend/assistant/`

4. Run the backend server:
```bash
python api.py
```
---

## Challenges We Faced

1. **AI Assistance**
   - The main challenge was **providing Gemini the ability to exert its control over the experiment**. Moreover, these actions must be    synchronized with the speech it was outputting. Thus, the llm can now turn on/off visualizations like electron flow, potential gradients etc., and place apparatuses on the command of the user.
   
2. **Intuitive Design Decisions**
   - 3D, Science Experiments and AR -- all together posed tricky design problems through out development. They varied from 
      > "How would the user try to do X when they are using the app for the first time?" 
      
      to 
      > "How do we model things from the real world without being too much or too little detailed?"
      
3. **Performance**
   - 3D rendering, web requests and AR simultaneously on mobile seemed like they might cause performance issues. But with optimization in the models and lighting, even a 5 year old android can have a decent performance.

---