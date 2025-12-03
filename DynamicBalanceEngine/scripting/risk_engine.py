import sys
import json

def calculate_risk(user_logs):
    """
    Calculates the risk score (R) for a user based on their logs.
    
    Formula: R = (ErrorCount * 1.5) + (ActionFrequency * 0.1)
    If R > 50, mark user as High Risk.
    
    Args:
        user_logs (list): A list of dictionaries representing user logs.
                          Each log should have keys like 'is_error' (bool).
                          
    Returns:
        dict: A dictionary containing 'risk_score' (float) and 'is_high_risk' (bool).
    """
    
    error_count = 0
    action_frequency = len(user_logs)
    
    for log in user_logs:
        if log.get('is_error', False):
            error_count += 1
            
    risk_score = (error_count * 1.5) + (action_frequency * 0.1)
    
    is_high_risk = risk_score > 50
    
    return {
        'risk_score': risk_score,
        'is_high_risk': is_high_risk
    }

if __name__ == "__main__":
    try:
        # Read from stdin
        input_data = sys.stdin.read()
        if not input_data:
            # Fallback for testing if no input
            sample_logs = [{'is_error': False}, {'is_error': True}]
            print(json.dumps(calculate_risk(sample_logs)))
        else:
            user_logs = json.loads(input_data)
            result = calculate_risk(user_logs)
            print(json.dumps(result))
    except Exception as e:
        print(json.dumps({'error': str(e)}), file=sys.stderr)
        sys.exit(1)
