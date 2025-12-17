import argparse
import json
import random
import uuid
from datetime import datetime

def simulate_brute_force(target_user_id=None, count=20):
    """
    Simulates a brute-force attack by generating logs with high failure rates.
    """
    if not target_user_id:
        target_user_id = str(uuid.uuid4())
        
    logs = []
    print(f"Starting brute-force simulation for Target User: {target_user_id}")
    
    for i in range(count):
        # Brute force pattern: High failure, high frequency
        log_entry = {
            "user_id": target_user_id,
            "timestamp": datetime.now().isoformat(),
            "failed_logins": random.randint(5, 15), # High number of failures
            "geo_change_detected": False,
            "high_freq_requests": True, # Always true for brute force
            "transaction_amount": 0
        }
        logs.append(log_entry)
        
    return logs

def main():
    parser = argparse.ArgumentParser(description="Simulate Brute Force Attack")
    parser.add_argument("--user_id", type=str, help="Target User ID (optional)")
    parser.add_argument("--count", type=int, default=20, help="Number of attack attempts")
    parser.add_argument("--output", type=str, default="attack_logs.json", help="Output file path")
    
    args = parser.parse_args()
    
    logs = simulate_brute_force(args.user_id, args.count)
    
    try:
        with open(args.output, 'w', encoding='utf-8') as f:
            json.dump(logs, f, indent=4)
        print(f"ATTACK SIMULATION COMPLETE. {len(logs)} malicious logs generated to {args.output}")
    except Exception as e:
        print(f"Error writing to file: {e}")

if __name__ == "__main__":
    main()
