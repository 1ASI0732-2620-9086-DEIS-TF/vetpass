Feature: US09 - Automatic generation of the vaccination card
  As clinic staff, I want the vaccination card to be generated when a pet is registered,
  so that I do not have to define the doses it needs.

  Background:
    Given the clinic staff is signed in
    And today is "2026-10-07"

  Scenario Outline: E1 - Generation by species
    When the staff registers the "<species>" "Bolt" born on "2026-08-26" for "Jorge Aliaga"
    Then the card of "Bolt" has <doses> pending doses

    Examples:
      | species | doses |
      | Canine  | 6     |
      | Feline  | 9     |

  Scenario: E2 - Expected dates
    When the staff registers the "Canine" "Bolt" born on "2026-09-09" for "Jorge Aliaga"
    Then the doses of "Bolt" are expected on
      | vaccine     | dose | date       |
      | Quíntuple   | 1    | 2026-10-21 |
      | Quíntuple   | 2    | 2026-11-11 |
      | Antirrábica | 1    | 2026-12-02 |

  Scenario: E3 - Pet older than the first doses
    When the staff registers the "Canine" "Max" born on "2022-10-07" for "Jorge Aliaga"
    Then the doses of "Max" are expected on
      | vaccine     | dose | date       |
      | Quíntuple   | 1    | 2026-10-07 |
      | Antirrábica | 1    | 2026-10-07 |
      | Quíntuple   | 2    | 2026-10-28 |
      | Quíntuple   | 3    | 2026-11-18 |
