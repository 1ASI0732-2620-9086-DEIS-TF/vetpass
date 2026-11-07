Feature: US04 - Authentication of the clinic staff
  As clinic staff, I want to sign in to the web application,
  so that I can reach the information of my patients.

  Scenario: E1 - Valid credentials
    When the user "andrea.quispe@vetsanmiguel.pe" signs in with the demonstration password
    Then the session has the role "ClinicStaff"

  Scenario Outline: E2 - Invalid credentials
    When the user "<email>" signs in with the password "Incorrecta.2026"
    Then the request is rejected with status 401
    And the answer opens no session

    Examples:
      | email                         |
      | andrea.quispe@vetsanmiguel.pe |
      | nadie@vetsanmiguel.pe         |
