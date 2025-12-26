import random
import time
import requests
import json
import uuid

# Configuration
API_URL = "http://localhost:5000/api/logs" # Adjust port as needed
USER_ID = str(uuid.uuid4()) # Simulate a user ID
NUM_LOGS = 20

def generate_log():
    actions = ["Login", "FileUpload", "APIRequest"]
    is_error = random.choice([True, False])
    
    log = {
        "UserId": USER_ID,
        "ActionType": random.choice(actions),
        "IsError": is_error,
        "Details": "Simulated log entry"
    }
    return log

def send_logs():
    print(f"Generating and sending {NUM_LOGS} logs for User {USER_ID}...")
    
    for _ in range(NUM_LOGS):
        log = generate_log()
        
        # In a real scenario, you would send this to the API
        # response = requests.post(API_URL, json=log)
        # print(f"Sent log: {log}, Status: {response.status_code}")
        
        # For this standalone script, we'll just print it or save to a file
        print(f"Generated Log: {json.dumps(log)}")
        time.sleep(0.1) # Simulate delay

if __name__ == "__main__":
    send_logs()
