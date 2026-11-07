Feature: US10 - Registration of an applied dose
  As clinic staff, I want to register the application of a dose with its date, batch and veterinarian,
  so that the card keeps verifiable evidence.

  Background:
    Given the clinic staff is signed in
    And today is "2026-10-07"

  Scenario: E1 - Valid registration
    When the staff registers dose 2 of "Quíntuple" for "Rocky" applied on "2026-10-07" with batch "a-4521"
    Then the dose is registered as applied with batch "A-4521"

  Scenario: E2 - Minimum age not reached
    When the staff registers dose 1 of "Antirrábica" for "Rocky" applied on "2026-10-07" with batch "R-1300"
    Then the request is rejected with the code "minimum-age-not-reached"
    And the earliest admissible date is "2026-10-28"

  Scenario: E3 - Minimum interval not met
    Given the "Canine" "Bolt" born on "2026-06-01" is registered for "Jorge Aliaga"
    And dose 1 of "Quíntuple" for "Bolt" was applied on "2026-09-01"
    When the staff registers dose 2 of "Quíntuple" for "Bolt" applied on "2026-09-15" with batch "A-1"
    Then the request is rejected with the code "minimum-interval-not-met"
    And the earliest admissible date is "2026-09-22"

  Scenario: E4 - Future application date
    When the staff registers dose 2 of "Quíntuple" for "Rocky" applied on "2026-10-08" with batch "A-1"
    Then the request is rejected with the code "future-application-date"

  Scenario: E5 - Dose out of order
    When the staff registers dose 3 of "Quíntuple" for "Rocky" applied on "2026-10-07" with batch "A-1"
    Then the request is rejected with the code "dose-out-of-order"

  Scenario: E5 - Doses of different vaccines on the same day
    Given the "Canine" "Bolt" born on "2026-06-01" is registered for "Jorge Aliaga"
    When the staff registers dose 1 of "Quíntuple" for "Bolt" applied on "2026-10-07" with batch "A-1"
    And the staff registers dose 1 of "Antirrábica" for "Bolt" applied on "2026-10-07" with batch "R-1"
    Then the dose is registered as applied with batch "R-1"

  Scenario: E6 - Dose applied before the registration
    Given the "Canine" "Max" born on "2022-10-07" is registered for "Jorge Aliaga"
    When the staff registers dose 1 of "Quíntuple" for "Max" applied on "2022-11-25" with batch "H-2022"
    Then the dose is registered as applied with batch "H-2022"
    And the doses of "Max" are expected on
      | vaccine   | dose | date       |
      | Quíntuple | 2    | 2026-10-07 |
