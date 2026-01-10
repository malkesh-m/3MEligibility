import { inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root' // Ensures service is available globally
})
export class UtilityService {
  /**
   * Sanitizes input by allowing only alphanumeric characters and a single hyphen (-).
   * @param input - The input string to sanitize.
   * @returns Sanitized string with only A-Z, a-z, 0-9, and - allowed.
   */
  //sanitizeCode(input: string): string {
  //  return input.replace(/[^A-Za-z0-9-\s]/g, ''); // Removes all special characters except '-'
  //}
  sanitizeCode(input: string, allowComma: boolean = false, dataType: string = ''): string {
    if (dataType === 'Numeric') {
      // Allow only digits and comma if SelectMultiple is true
      return allowComma ? input.replace(/[^0-9,]/g, '') : input.replace(/[^0-9]/g, '');
    }
    const regex = allowComma
      ? /[^a-zA-Z\u0600-\u06FF0-9,\s]/g
      : /[^a-zA-Z\u0600-\u06FF0-9\s]/g;

    return input.replace(regex, '');
  }
  /**
   * Additional utility functions can be added here...
  */
}
