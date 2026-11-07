Feature: US12 - Vaccination card from the mobile application
  As a pet owner, I want to check the vaccination card of each of my pets from my phone,
  so that I know what each one needs without depending on the paper document.

  Background:
    Given the owner "valeria.campos@correo.com" is signed in
    And today is "2026-10-07"

  Scenario: E1 - Card of a pet
    When the user requests the vaccination card of "Kiara"
    Then the status of the card is "UpToDate"
    And the card shows applied doses and pending doses

  Scenario: E2 - Each pet has its own status
    When the owner lists their pets
    Then the status of each pet is
      | pet   | status   |
      | Kiara | UpToDate |
      | Simón | Overdue  |
