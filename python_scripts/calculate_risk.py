import sys
import json
import re

def calculate_risk(input_data):
    """
    Calculates risk score based on policy:
    - Login Fail: +5, Success: -2
    - Post/Comment Flood (>5 in 5m): +15
    - Content Threat (XSS/SQLi): +30
    - Safe Post: -5
    - Safe Comment: -2 (implicit safe behavior)
    """
    try:
        data = json.loads(input_data)
        current_score = data.get("current_score", 0)
        action = data.get("action", "unknown")
        status = data.get("status", "success")
        frequency = data.get("frequency", 0)
        content = data.get("content", "")
        
        new_score = current_score
        issues = []
        is_content_threat = False

        # 1. BASELINE
        if action == "login":
            if status == "failure":
                new_score += 15  # Brute force attempt risk
                issues.append("Login Failure")
            else:
                new_score = max(0, new_score - 2)
                
        elif action == "time_decay":
            # Handled by caller (backend), but if script called, confirm reduction
            pass 

        # 2. CONTENT ANALYSIS (If safe action)
        if action in ["create_post", "create_comment"]:
            # A. SQL Injection & XSS Patterns (CRITICAL)
            sqli_xss_patterns = [
                r"<script>", r"javascript:", r"onerror=", r"onload=",  # XSS
                r"'\s*OR\s*['\"]?1['\"]?\s*=\s*['\"]?1",               # SQLi: ' OR '1'='1
                r"'\s*OR\s*1\s*=\s*1",                                  # SQLi: ' OR 1=1
                r"UNION\s+SELECT",                                      # SQLi: UNION SELECT
                r"'\s*--", r'"\s*--', r";\s*DROP\s+TABLE"               # Destructive SQLi
            ]
            
            for pattern in sqli_xss_patterns:
                if re.search(pattern, content, re.IGNORECASE):
                    new_score = 95 # INSTANT CRITICAL
                    issues.append(f"Critical Security Threat (SQLi/XSS)")
                    is_content_threat = True
                    break

            # B. SPAM & BEHAVIOR (If not already critical)
            if not is_content_threat:
                # Caps Lock Check (>60% caps and len > 5)
                letters = [c for c in content if c.isalpha()]
                if len(letters) > 5:
                    upper_count = sum(1 for c in letters if c.isupper())
                    if upper_count / len(letters) > 0.6: # Lowered threshold
                        new_score += 10
                        issues.append("Spam Behavior (CAPSLOCK)")
                
                # Link/Ad Check
                if re.search(r"http[s]?://|www\.|[a-zA-Z0-9-]+\.(com|net|org)", content, re.IGNORECASE):
                    new_score += 15
                    issues.append("Potential Advertising/Phishing")

                # Length Anomalies (Too short)
                if len(content) < 3:
                     # Penalize very short spam
                     new_score += 2
                     issues.append("Low Quality Content")

                # Standard Action Reduction (Reward for 'good' behavior if no issues)
                if not issues:
                    new_score = max(0, new_score - 2)

        # 3. FREQUENCY (FLOODING)
        if frequency > 5:
            flood_penalty = (frequency - 5) * 10
            new_score += flood_penalty
            issues.append(f"Flooding Detected (+{flood_penalty} pts)")
            
        # 4. CAP & RETURN
        new_score = min(100, max(0, new_score))
        risk_delta = new_score - current_score

        return json.dumps({
            "new_score": new_score,
            "risk_delta": risk_delta,
            "issues": issues
        })

    except Exception as e:
        return json.dumps({"error": str(e), "new_score": current_score, "risk_delta": 0})

if __name__ == "__main__":
    if len(sys.argv) > 1:
        print(calculate_risk(sys.argv[1]))
    else:
        print(json.dumps({"error": "No input provided"}))
