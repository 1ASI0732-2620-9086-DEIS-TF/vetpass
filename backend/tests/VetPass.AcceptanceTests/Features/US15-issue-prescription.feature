Feature: US15 - Issue of a prescription
  As clinic staff, I want to issue a prescription linked to a visit,
  so that the owner has the indication in writing.

  Background:
    Given the clinic staff is signed in
    And today is "2026-10-07"
    And the staff registered a visit for "Kiara" on "2026-10-07" with reason "Control" and diagnosis "Sano"

  Scenario: E1 - Successful issue
    When the staff issues a prescription of "Amoxicilina 250 mg" with dosage "Una tableta cada 12 horas" for "7 días"
    Then the prescription is issued
    And the owner "valeria.campos@correo.com" sees "Amoxicilina 250 mg" in the prescription of the visit

  Scenario: E2 - Prescription without medications
    When the staff issues a prescription without medications
    Then the request is rejected with status 400
