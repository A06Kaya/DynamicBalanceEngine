import pytest
from scripting.calculate_risk import calculate_risk

def test_base_risk():
    result = calculate_risk(0, 0, False, False)
    assert result["risk_score"] == 0
    assert result["risk_level"] == "LOW"

def test_failed_logins_risk():
    # 5 failed logins * 10 = 50 -> HIGH
    result = calculate_risk(5, 0, False, False)
    assert result["risk_score"] == 50
    assert result["risk_level"] == "HIGH"

def test_geo_change_risk():
    # 20 points
    result = calculate_risk(0, 0, True, False)
    assert result["risk_score"] == 20
    assert result["risk_level"] == "MEDIUM"

def test_high_freq_risk():
    # 5 points
    result = calculate_risk(0, 0, False, True)
    assert result["risk_score"] == 5
    assert result["risk_level"] == "LOW"

def test_combined_risk():
    # 1 failed (10) + Geo (20) + High Freq (5) = 35 -> MEDIUM
    result = calculate_risk(1, 0, True, True)
    assert result["risk_score"] == 35
    assert result["risk_level"] == "MEDIUM"

def test_transaction_risk():
    # 2000 amount -> +2 points
    result = calculate_risk(0, 2000, False, False)
    assert result["risk_score"] == 2
