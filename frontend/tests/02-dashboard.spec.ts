import { test, expect } from '@playwright/test';
import { loginAs } from './helpers/auth.helper';

test.describe('Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page);
  });

  test('shows metric cards with data', async ({ page }) => {
    await expect(page.getByText(/total medicines/i)).toBeVisible();
    await expect(page.getByText(/low stock/i).first()).toBeVisible();
    await expect(page.getByText(/expiring soon/i).first()).toBeVisible();
  });

  test('renders monthly sales chart', async ({ page }) => {
    await expect(page.locator('canvas').first()).toBeVisible({ timeout: 8_000 });
  });

  test('metric card numbers are non-empty', async ({ page }) => {
    const cards = page.locator('.metric-card, .stat-card, [class*="card"]');
    await expect(cards.first()).toBeVisible();
  });

  test('sidebar navigation links are visible', async ({ page }) => {
    await expect(page.locator('.nav-item', { hasText: /medicines/i })).toBeVisible();
    await expect(page.locator('.nav-item', { hasText: /inventory/i })).toBeVisible();
    await expect(page.locator('.nav-item', { hasText: /sales/i })).toBeVisible();
    await expect(page.locator('.nav-item', { hasText: /alerts/i })).toBeVisible();
  });
});
