Feature: US08 - Patient search
  As clinic staff, I want to locate a pet by its name or by the name of its owner,
  so that I can reach its record when an appointment starts.

  Background:
    Given the clinic staff is signed in

  Scenario Outline: E1 - Matches found
    When the staff searches for patients with "<criterion>"
    Then the patients found are "<pets>"
    And every patient found shows its species and the owner "<owner>"

    Examples:
      | criterion | pets         | owner          |
      | kiara     | Kiara        | Valeria Campos |
      | Valeria   | Kiara, Simón | Valeria Campos |

  Scenario: E2 - No matches
    When the staff searches for patients with "Inexistente"
    Then no patient is found
