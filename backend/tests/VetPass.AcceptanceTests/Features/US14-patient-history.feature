Feature: US14 - History of the patient
  As clinic staff, I want to check the previous visits of a patient in chronological order,
  so that I can decide with complete information.

  Background:
    Given the clinic staff is signed in

  Scenario: E1 - History with visits
    When the user requests the history of "Rocky"
    Then the visits are, from the most recent
      | reason                                       |
      | Primera consulta y primera dosis del esquema |
      | Control de ingreso                           |

  Scenario: E2 - Empty history
    When the user requests the history of "Luna"
    Then the history has no visits
