import urllib.request
import json
import sys

# Internal port is 8080
base_url = "http://localhost:8080"

print("Logging in...")
url = f"{base_url}/api/Auth/login"
data = json.dumps({"username":"ahmet", "password":"password"}).encode('utf-8')
req = urllib.request.Request(url, data=data, headers={'Content-Type': 'application/json'})

try:
    with urllib.request.urlopen(req) as f:
        resp = json.loads(f.read().decode('utf-8'))
        token = resp['token']
        print(f"Got Token: {token[:20]}...")
        
        print("Requesting /me...")
        url_me = f"{base_url}/api/Auth/me"
        req_me = urllib.request.Request(url_me, headers={'Authorization': f"Bearer {token}"})
        try:
            with urllib.request.urlopen(req_me) as f_me:
                print(f"SUCCESS: {f_me.status_code}")
                print(f_me.read().decode('utf-8'))
        except urllib.error.HTTPError as e:
            print(f"Me Error: {e.code}")
            print(e.read().decode('utf-8'))

except Exception as e:
    print(f"Login Error: {e}")
