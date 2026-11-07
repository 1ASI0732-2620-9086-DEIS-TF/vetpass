Feature: US18 - Password reset by the clinic
  As clinic staff, I want to reset the password of a client who forgot it,
  so that they recover access to the mobile application without the clinic knowing the password they choose.

  Scenario: E1 - Successful reset
    Given the clinic staff is signed in
    When the staff resets the password of "Valeria Campos"
    Then a temporary password of 12 characters is issued
    And the owner "valeria.campos@correo.com" signs in with the temporary password but not with the previous one

  Scenario: E2 - Password not visible
    Given the clinic staff is signed in
    When the staff lists the clients
    Then no client shows a password

  Scenario: E3 - Client without access
    Given the clinic staff is signed in
    When the staff resets the password of "Carlos Ramos"
    Then the request is rejected with the code "client-without-account"

  Scenario: E4 - Client of another clinic
    Given the staff of another clinic is signed in
    When the staff resets the password of "Valeria Campos"
    Then the request is denied
