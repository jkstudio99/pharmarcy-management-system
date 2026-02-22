import { Page } from '@playwright/test';

export const ADMIN = { email: 'admin@pharmacy.com', password: 'Admin@123' };
export const BASE = 'http://localhost:4200';

export async function loginAs(page: Page, email = ADMIN.email, password = ADMIN.password) {
  await page.goto('/login');
  await page.waitForLoadState('networkidle');
  await page.getByPlaceholder('admin@pharmacy.com').fill(email);
  await page.locator('input[name="password"]').fill(password);
  await page.getByRole('button', { name: /sign in/i }).click();
  await page.waitForURL('**/dashboard', { timeout: 15_000 });
}

export async function clearSession(page: Page) {
  await page.goto('/login');
  await page.evaluate(() => {
    localStorage.removeItem('pharma_token');
    localStorage.removeItem('pharma_user');
  });
}
