Feature: US11 - Status of the vaccination card
  As clinic staff, I want to know the status of the card of a patient,
  so that I can decide what it needs before attending it.

  Background:
    Given the clinic staff is signed in
    And today is "2026-10-07"

  Scenario: E1 - Card up to date
    When the user requests the vaccination card of "Kiara"
    Then the status of the card is "UpToDate"

  Scenario: E2 - Card pending
    When the user requests the vaccination card of "Rocky"
    Then the status of the card is "Pending"
    And the next dose is dose 2 of "Quíntuple"

  Scenario: E3 - Card overdue
    When the user requests the vaccination card of "Toby"
    Then the status of the card is "Overdue"
