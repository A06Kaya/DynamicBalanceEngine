import pytest
from scripting.calculate_risk import calculate_risk

def test_base_risk():
    result = calculate_risk(0, 0, False, False)
    assert result["risk_score"] == 0
    assert result["risk_level"] == "LOW"

def test_failed_logins_risk():
    # 5 failed logins * 20 = 100 -> HIGH
    result = calculate_risk(5, 0, False, False)
    assert result["risk_score"] == 100
    assert result["risk_level"] == "HIGH"

def test_geo_change_risk():
    # 20 points
    result = calculate_risk(0, 0, True, False)
    assert result["risk_score"] == 20
    assert result["risk_level"] == "MEDIUM"

def test_high_freq_risk():
    # 30 points (New Base)
    result = calculate_risk(0, 0, False, True)
    assert result["risk_score"] == 30
    assert result["risk_level"] == "MEDIUM"

def test_combined_risk():
    # 1 failed (20) + Geo (20) + High Freq (30) = 70 -> HIGH
    result = calculate_risk(1, 0, True, True)
    assert result["risk_score"] == 70
    assert result["risk_level"] == "HIGH"

def test_transaction_risk():
    # 2000 amount -> +2 points
    result = calculate_risk(0, 2000, False, False)
    assert result["risk_score"] == 2
    
def test_aggressive_penalty():
    # 6 failed (120) + High Freq (30 + 50 extra) = 200 -> HIGH
    result = calculate_risk(6, 0, False, True)
    assert result["risk_score"] == 200
    assert result["risk_level"] == "HIGH"
