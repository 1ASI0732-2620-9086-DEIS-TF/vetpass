Feature: US07 - Pet registration
  As clinic staff, I want to register a pet linked to a client, with its species and birth date,
  so that it becomes a patient of the clinic.

  Background:
    Given the clinic staff is signed in

  Scenario: E1 - Successful registration
    When the staff registers the "Canine" "Bolt" born on "2026-06-01" for "Jorge Aliaga"
    Then the pet is registered with its vaccination card

  Scenario: E2 - Unsupported species
    When the staff registers the "Rabbit" "Bolt" born on "2026-06-01" for "Jorge Aliaga"
    Then the request is rejected with the code "unsupported-species"

  Scenario: E3 - Future birth date
    When the staff registers the "Canine" "Bolt" born on "2026-10-08" for "Jorge Aliaga"
    Then the request is rejected with the code "future-birth-date"

  Scenario: E4 - Implausible birth date
    When the staff registers the "Canine" "Bolt" born on "1990-01-01" for "Jorge Aliaga"
    Then the request is rejected with the code "implausible-birth-date"
