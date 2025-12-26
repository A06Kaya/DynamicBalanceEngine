import random
import json
import time
import requests

# Configuration
API_URL = "http://localhost:5000/api/Auth/login"
USERS = [
    {"username": "testuser1", "password": "password"},
    {"username": "testuser2", "password": "password"},
    # User 3 will fail often
    {"username": "hacker", "password": "wrongpassword"} 
]

def generate_traffic():
    print("Starting traffic generation...")
    while True:
        user = random.choice(USERS)
        
        # Decide if we send a correct password or not
        password = user["password"]
        if user["username"] == "hacker":
            if random.random() < 0.9: # 90% chance to fail
                password = "wrong_" + password
        
        payload = {
            "username": user["username"],
            "password": password
        }
        
        try:
            response = requests.post(API_URL, json=payload)
            print(f"User: {user['username']} | Status: {response.status_code} | Response: {response.text}")
        except Exception as e:
            print(f"Error: {e}")
            
        time.sleep(1)

if __name__ == "__main__":
    generate_traffic()
