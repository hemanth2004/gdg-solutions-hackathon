import os
import warnings
import textwrap
from dotenv import load_dotenv
from pathlib import Path
from IPython.display import Markdown
from langchain_core.prompts import PromptTemplate
from langchain.chains import RetrievalQA
from langchain_community.document_loaders import PyPDFLoader
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_community.vectorstores import Chroma
from langchain_google_genai import ChatGoogleGenerativeAI, GoogleGenerativeAIEmbeddings

load_dotenv()

warnings.filterwarnings("ignore")

def to_markdown(text):
    text = text.replace('•', '  *')
    return Markdown(textwrap.indent(text, '> ', predicate=lambda _: True))

def setup_qa_system(pdf_path, google_api_key):
    # Load and split PDF
    pdf_loader = PyPDFLoader(pdf_path)
    pages = pdf_loader.load_and_split()
    
    if not pages:
        raise ValueError("Error: No pages found in the PDF. Please check the file.")

    context = "\n\n".join(str(p.page_content) for p in pages)
    
    # Split text into chunks
    text_splitter = RecursiveCharacterTextSplitter(chunk_size=10000, chunk_overlap=1000)
    texts = text_splitter.split_text(context)

    if not texts:
        raise ValueError("Error: No text extracted from the PDF.")

    # Create embeddings and vector store
    embeddings = GoogleGenerativeAIEmbeddings(model="models/embedding-001", google_api_key=google_api_key)
    
    try:
        vector_index = Chroma.from_texts(texts, embeddings).as_retriever(search_kwargs={"k": 5})
    except ValueError as e:
        raise ValueError("Error: Embeddings could not be generated. Check API key and input text.") from e

    # Configure model
    model = ChatGoogleGenerativeAI(model="gemini-2.0-flash", google_api_key=google_api_key, temperature=0.2, convert_system_message_to_human=True)
    
    # Define prompt template
    template = """Use the following pieces of context to answer the question at the end. While presenting, say "Based on my knowledge" at the beginning.
    If you don't know the answer, just say that you don't know. Don't try to make up an answer. Keep the answer concise.
    Always say "For any experiment-related assistance, ask our AI Lab Assistant!" at the end of the answer.
    {context}
    Question: {question}
    Helpful Answer:"""
    
    QA_CHAIN_PROMPT = PromptTemplate.from_template(template)
    
    # Create QA chain
    qa_chain = RetrievalQA.from_chain_type(
        model,
        retriever=vector_index,
        return_source_documents=True,
        chain_type_kwargs={"prompt": QA_CHAIN_PROMPT}
    )
    
    return qa_chain

def ask_question(qa_chain, question):
    result = qa_chain({"query": question})
    return result["result"]

# Example usage
if __name__ == "__main__":
    GOOGLE_API_KEY = os.getenv("GOOGLE_API_KEY")
    PDF_PATH = "ARLABS_troubleshoot_GUIDE.pdf"

    if not os.path.exists(PDF_PATH):
        raise FileNotFoundError(f"Error: The file '{PDF_PATH}' does not exist.")

    qa_chain = setup_qa_system(PDF_PATH, GOOGLE_API_KEY)
    
    question = "why ai is not responding?"
    answer = ask_question(qa_chain, question)
    print(answer)
