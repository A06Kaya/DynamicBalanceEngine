# ⚖️ The Dynamic Balance Engine

> **"Perfect security is not a wall, but a dynamic equilibrium."**

Welcome to the **Dynamic Balance Engine** project repository. This system is designed to solve the eternal dilemma between **Security** and **Freedom** by implementing an adaptive, real-time risk assessment mechanism.

## 🚀 Project Overview

The Dynamic Balance Engine continuously analyzes user behavior to calculate a **Risk Score (R)**. Based on this score, the system dynamically adjusts security protocols—granting freedom to low-risk users while tightening defenses for high-risk anomalies.

### 🧠 Core Logic (Scripting Layer)
This branch (`karasungur-dev`) focuses on the **Python Scripting Layer**, which acts as the "brain" of the operation.

#### Key Components:
1.  **`calculate_risk.py`**: The engine that processes user data and outputs a Risk Score.
    *   *Inputs:* Failed logins, Geo-location changes, Transaction amounts.
    *   *Outputs:* Risk Score (0-100+) and Risk Level (LOW, MEDIUM, HIGH).
2.  **`data_generator.py`**: A utility to generate synthetic user logs for testing and simulation.

---

## 🛠️ Getting Started

### Prerequisites
- Python 3.x
- `pip`

### Installation
1.  Clone the repository.
2.  Install dependencies:
    ```bash
    pip install -r scripting/requirements.txt
    ```

### 🕹️ Usage

#### 1. Calculate Risk (Manual Test)
```bash
python scripting/calculate_risk.py --failed_logins 3 --geo_change
```

#### 2. Generate Dummy Data
Generate 10 random user logs for testing:
```bash
python scripting/data_generator.py --count 10 --output user_logs.json
```

#### 3. Run Tests
Verify the integrity of the risk engine:
```bash
python -m pytest scripting/tests/
```

---

## 👥 Team 5
- **Ahmet Kaya** (Scrum Master & Backend)
- **Mustafa Karasungur** (Scripting & Logic)
- **Erdem Ulu** (Backend & Database)
- **Mustafa Sina Yıldırır** (Research & Docs)

---
*Gazi University - Computer Engineering - Secure Coding & Scripting Languages Project*
