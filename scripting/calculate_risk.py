import argparse
import json
import sys
from datetime import datetime

def calculate_risk(failed_logins, transaction_amount, geo_change_detected, high_freq_requests):
    """
    Calculates the Risk Score (R) based on user behavior.
    
    Base Score: 0
    Rules:
    - +10 per failed login attempt
    - +20 if significant geo-location change is detected
    - +5 if high frequency requests are detected
    - +1 per $1000 in transaction amount (simulated financial risk)
    
    Returns:
        dict: containing risk_score and risk_level
    """
    risk_score = 0
    
    # Rule 1: Failed Logins
    risk_score += failed_logins * 10
    
    # Rule 2: Geo Change
    if geo_change_detected:
        risk_score += 20
        
    # Rule 3: High Frequency Requests
    if high_freq_requests:
        risk_score += 5
        
    # Rule 4: Transaction Amount Risk
    if transaction_amount > 0:
        risk_score += int(transaction_amount / 1000)

    # Determine Risk Level
    if risk_score < 20:
        risk_level = "LOW"
    elif risk_score < 50:
        risk_level = "MEDIUM"
    else:
        risk_level = "HIGH"
        
    return {
        "risk_score": risk_score,
        "risk_level": risk_level,
        "details": {
            "failed_logins_penalty": failed_logins * 10,
            "geo_change_penalty": 20 if geo_change_detected else 0,
            "high_freq_penalty": 5 if high_freq_requests else 0,
            "transaction_penalty": int(transaction_amount / 1000)
        }
    }

def main():
    parser = argparse.ArgumentParser(description="Calculate User Risk Score")
    parser.add_argument("--failed_logins", type=int, default=0, help="Number of failed login attempts")
    parser.add_argument("--transaction_amount", type=float, default=0.0, help="Transaction amount involved")
    parser.add_argument("--geo_change", action="store_true", help="Flag if significant geo-location change detected")
    parser.add_argument("--high_freq", action="store_true", help="Flag if high frequency requests detected")
    
    args = parser.parse_args()
    
    result = calculate_risk(
        failed_logins=args.failed_logins,
        transaction_amount=args.transaction_amount,
        geo_change_detected=args.geo_change,
        high_freq_requests=args.high_freq
    )
    
    print(json.dumps(result, indent=4))

if __name__ == "__main__":
    main()
