Feature: US05 - Pet owner access to the mobile application
  As a pet owner, I want to sign in to the mobile application,
  so that I can consult the information of my pets.

  Scenario: E1 - Access granted
    When the owner "valeria.campos@correo.com" signs in with the password given by the clinic
    Then the session has the role "PetOwner"
    And the owner sees the pets "Kiara, Simón"

  Scenario: E2 - Isolation of information
    Given the owner "valeria.campos@correo.com" is signed in
    When the user requests the vaccination card of "Rocky"
    Then the request is denied
