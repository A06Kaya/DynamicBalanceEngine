# DynamicBalanceEngine

This project is an adaptive balance mechanism that addresses the core dilemma of digital systems: the tension between absolute security and user freedom. It dynamically adjusts security measures based on real-time risk assessment, allowing for a fluid user experience while maintaining robust protection.

## Features

- **Adaptive Limit Management**: Dynamically adjusts transaction limits and security thresholds based on user behavior and risk scores.
- **Real-time Risk Calculation**: Utilizes Python scripts to calculate risk scores using factors like location, device, and transaction history.
- **Security Rules**: Implements specific security rules (e.g., SQL Injection detection, Caps Lock usage patterns) to identify potential threats.
- **Dockerized Environment**: Fully containerized setup using Docker and Docker Compose for easy deployment.

## Technology Stack

- **Backend**: .NET 8 (C#)
- **Scripting/Analysis**: Python
- **Database**: PostgreSQL
- **Containerization**: Docker & Docker Compose

## Project Structure

- `backend/`: Contains the .NET Web API application code.
- `python_scripts/`: Python scripts for risk calculation (`risk_calculator.py`) and data generation.
- `frontend/`: Frontend application code.
- `docker-compose.yml`: Orchestrates the services (database, backend, etc.).

## Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

### Installation & Run

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd DynamicBalanceEngine
   ```

2. Build and start the services using Docker Compose:
   ```bash
   docker-compose up --build
   ```

3. The API will be available at standard ports (check `docker-compose.yml` for specifics, usually `http://localhost:8080` or `5000`).

## Development

- **Run Tests**: PowerShell scripts are provided for testing various functionalities (e.g., `test_auth.ps1`, `verify_risk_logic.ps1`).
- **Debugging**: Python scripts can be debugged independently or integrated via the backend calls.
