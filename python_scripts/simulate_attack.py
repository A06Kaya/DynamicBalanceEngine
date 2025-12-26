import requests
import time

API_URL = "http://localhost:5000/api/Auth/login"

def attack():
    print("Starting BRUTE FORCE attack simulation...")
    target_user = "testuser1"
    
    # Rapid fire wrong passwords
    for i in range(20):
        payload = {
            "username": target_user,
            "password": f"wrongpass{i}"
        }
        try:
            response = requests.post(API_URL, json=payload)
            print(f"Attack attempt {i+1}: {response.status_code} - {response.text}")
        except Exception as e:
            print(f"Error: {e}")
        time.sleep(0.1) # Fast

if __name__ == "__main__":
    attack()
