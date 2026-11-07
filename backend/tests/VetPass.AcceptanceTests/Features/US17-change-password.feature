Feature: US17 - Password change by the pet owner
  As a pet owner, I want to change my password from the mobile application,
  so that only I know the key to the information of my pets.

  Scenario: E1 - Successful change
    Given the owner "valeria.campos@correo.com" is signed in
    When the owner changes the password to "Perrito2026"
    Then the change is accepted
    And the owner signs in with "Perrito2026" but not with the previous password

  Scenario: E2 - Wrong current password
    Given the owner "valeria.campos@correo.com" is signed in
    When the owner changes the password to "Perrito2026" giving "NoEsLaClave1" as the current one
    Then the request is rejected with the code "invalid-current-password"
    And the session is still open

  Scenario: E3 - Weak password
    Given the owner "valeria.campos@correo.com" is signed in
    When the owner changes the password to "perrito"
    Then the request is rejected with the code "weak-password"

  Scenario: E4 - Temporary password
    Given the clinic gives mobile access to "Jorge Aliaga" with the email "jorge.aliaga@correo.com"
    When the owner "jorge.aliaga@correo.com" signs in with the temporary password
    Then the session requires a password change
