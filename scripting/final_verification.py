import json
import subprocess
import sys
import time
from datetime import datetime

RISK_SCRIPT = "scripting/calculate_risk.py"

def run_risk_calculation(failed_logins, high_freq, geo_change=False):
    """
    Runs the calculate_risk.py script via subprocess to simulate backend call.
    """
    cmd = [
        sys.executable, RISK_SCRIPT,
        "--failed_logins", str(failed_logins),
        "--transaction_amount", "0"
    ]
    if high_freq:
        cmd.append("--high_freq")
    if geo_change:
        cmd.append("--geo_change")
        
    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode != 0:
        print(f"Error executing script: {result.stderr}")
        return None
        
    return json.loads(result.stdout)

def verify_lifecycle():
    print(f"\n[{datetime.now().strftime('%H:%M:%S')}] --- STARTING FINAL SYSTEM VERIFICATION ---")
    
    # 1. Normal User Scenario
    print(f"\n[TEST 1] Normal User Behavior")
    res = run_risk_calculation(failed_logins=0, high_freq=False)
    print(f"   > Input: 0 failures, Normal frequency")
    print(f"   > Output: Score={res['risk_score']}, Level={res['risk_level']}")
    if res['risk_level'] == "LOW":
        print("   > [PASS] User is verified as LOW risk.")
    else:
        print("   > [FAIL] Expected LOW risk.")

    # 2. Suspicious Activity (High Frequency)
    print(f"\n[TEST 2] Suspicious Activity (Spam / High Freq)")
    res = run_risk_calculation(failed_logins=0, high_freq=True)
    print(f"   > Input: 0 failures, HIGH frequency")
    print(f"   > Output: Score={res['risk_score']}, Level={res['risk_level']}")
    if res['risk_score'] >= 30: # New base penalty
        print("   > [PASS] System detected High Frequency, increased score.")
    else:
        print("   > [FAIL] Score too low for high frequency.")

    # 3. Active Attack (Brute Force)
    print(f"\n[TEST 3] Active Attack (Brute Force + High Freq)")
    res = run_risk_calculation(failed_logins=10, high_freq=True)
    print(f"   > Input: 10 failures, HIGH frequency")
    print(f"   > Output: Score={res['risk_score']}, Level={res['risk_level']}")
    if res['risk_level'] == "HIGH":
        print("   > [PASS] User is BLOCKED (High Risk).")
    else:
        print("   > [FAIL] Expected HIGH risk.")

    print(f"\n[{datetime.now().strftime('%H:%M:%S')}] --- VERIFICATION COMPLETE ---")

if __name__ == "__main__":
    verify_lifecycle()
