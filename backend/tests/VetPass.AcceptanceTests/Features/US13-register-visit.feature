Feature: US13 - Registration of a veterinary visit
  As clinic staff, I want to register a visit with its reason, findings, diagnosis and treatment,
  so that the history of the patient is kept.

  Background:
    Given the clinic staff is signed in
    And today is "2026-10-07"

  Scenario: E1 - Successful registration
    When the staff registers a visit for "Rocky" on "2026-10-07" with reason "Control" and diagnosis "Sano"
    Then the visit is in the history of "Rocky" on "2026-10-07" with the staff as responsible

  Scenario Outline: E2 - Required fields missing
    When the staff registers a visit for "Rocky" on "2026-10-07" with reason "<reason>" and diagnosis "<diagnosis>"
    Then the request is rejected with status 400

    Examples:
      | reason  | diagnosis |
      |         | Sano      |
      | Control |           |
