import argparse
import json
import random
import uuid
from datetime import datetime, timedelta

def generate_logs(count):
    """
    Generates a list of synthetic user behavior logs.
    """
    logs = []
    for _ in range(count):
        user_id = str(uuid.uuid4())
        failed_logins = random.randint(0, 10)
        # 20% chance of geo change
        geo_change = random.choice([True] if random.random() < 0.2 else [False])
        # 10% chance of high frequency
        high_freq = random.choice([True] if random.random() < 0.1 else [False])
        # Random transaction amount between 0 and 5000
        transaction_amount = round(random.uniform(0, 5000), 2)
        
        log_entry = {
            "user_id": user_id,
            "timestamp": datetime.now().isoformat(),
            "failed_logins": failed_logins,
            "geo_change_detected": geo_change,
            "high_freq_requests": high_freq,
            "transaction_amount": transaction_amount
        }
        logs.append(log_entry)
    return logs

def main():
    parser = argparse.ArgumentParser(description="Generate Dummy User Logs")
    parser.add_argument("--count", type=int, default=10, help="Number of log entries to generate")
    parser.add_argument("--output", type=str, default="user_logs.json", help="Output file path")
    
    args = parser.parse_args()
    
    logs = generate_logs(args.count)
    
    try:
        with open(args.output, 'w', encoding='utf-8') as f:
            json.dump(logs, f, indent=4)
        print(f"Successfully generated {len(logs)} logs to {args.output}")
    except Exception as e:
        print(f"Error writing to file: {e}")

if __name__ == "__main__":
    main()
