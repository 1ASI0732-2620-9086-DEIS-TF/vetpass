Feature: US06 - Client registration
  As clinic staff, I want to register a client with their contact details,
  so that I can link them to their pets later.

  Background:
    Given the clinic staff is signed in

  Scenario: E1 - Successful registration
    When the staff registers the client "Ana Torres" with DNI "71234567" and phone "912 345 678"
    Then the client is registered
    And the phone of the client is stored as "+51912345678"

  Scenario: E2 - Missing data
    When the staff registers the client "" with DNI "71234567" and phone "912 345 678"
    Then the request is rejected with status 400

  Scenario: E3 - Non-Peruvian phone
    When the staff registers the client "Ana Torres" with DNI "71234567" and phone "+1 202 555 0100"
    Then the request is rejected with the code "invalid-phone-number"

  Scenario: E4 - Document already registered
    When the staff registers the client "Valeria C." with DNI "45879123" and phone "912 345 678"
    Then the request is rejected as a duplicate of the client "Valeria Campos"
