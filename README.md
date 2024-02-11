# aiao: RAG-based CLI Chat for Quiz Generation

<img src="https://github.com/Rahtoken/aiao-chat-app-hack/assets/23293949/fa7c6d2c-c82c-43a6-8657-fdacf7339847" width=256 height=256 />

aiao is a command-line interface (CLI) application designed to help students generate quizzes from their OneNote knowledge base using a Retrieval-Augmented Generation (RAG) approach. This  tool leverages the Microsoft Graph API to access OneNote content and Azure AI services, including the OpenAI API, to dynamically create topical quizzes for effective learning and revision.

## Features

- **Integration with OneNote:** Directly fetches content from OneNote sections to ensure that quizzes are relevant and up-to-date.
- **Dynamic Quiz Generation:** Uses Azure OpenAI's powerful language models to generate multiple-choice questions based on the OneNote content.
- **Command-Line Interface:** Easy-to-use CLI for generating quizzes, making it accessible for students who prefer terminal-based applications.

## Prerequisites

Before using Aiao, ensure you have the following:

- .NET 5.0 or higher installed on your machine.
- Access to Azure AI services and a Microsoft 365 account with OneNote content.
- The Microsoft.Identity.Client, Microsoft.Graph, and Azure.AI.OpenAI libraries installed.

## Installation

1. Clone the Aiao repository to your local machine.
2. Navigate to the Aiao directory and build the project using the .NET CLI:
