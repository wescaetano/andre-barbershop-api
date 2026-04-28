# Design: Public Book Page + Welcome Banner

**Date:** 2026-04-27
**Status:** Approved

---

## Overview

Two independent frontend features:

1. **Public book page** — unauthenticated users can browse available slots; authentication is required only when confirming the appointment.
2. **Welcome banner** — a persistent banner on the client home page showing the user's name and role (Barbeiro or Cliente) after login.

---

## Feature 1: Public Book Page

### Goal

Allow any visitor to see available slots without needing an account. Only require login/register when the user tries to confirm an appointment.

### Routing Change

The `book` route is moved out of `PrivateRoute` and registered as a standalone public route at `/book`. It uses a minimal wrapper (no `ClientLayout`, no `BottomNav`). All other `/app/*` routes remain unchanged behind `PrivateRoute`.

**Before:**
```
/app (PrivateRoute)
  /book
  /appointments
  /profile
```

**After:**
```
/book  ← public, standalone
/app (PrivateRoute)
  /appointments
  /profile
```

The home route `/app` (index) stays inside `PrivateRoute` because it shows user-specific data (upcoming appointments).

### Block on Confirm Step

The `Book` component has a 3-step flow: **Data → Horário → Confirmar**.

On step 2 (Confirmar), if `isAuthenticated === false`, the confirmation summary and button are replaced by a modal with:

- Message: *"Para finalizar o agendamento, crie uma conta ou entre na sua."*
- Button **Criar conta** → navigates to `/login?mode=register`
- Button **Entrar** → navigates to `/login`
- Both buttons pass the current selection via React Router `state`: `{ date, slot, returnTo: '/book' }` so the login page can redirect back with context preserved.

The slots grid and calendar remain fully functional for unauthenticated users.

### Backend consideration

The endpoint `GET /appointments/available` must accept unauthenticated requests. If it currently requires a JWT, the `[Authorize]` attribute on that endpoint must be removed or replaced with `[AllowAnonymous]`.

### Navigation

- The `BottomNav` on `/app` links "Agendar" to `/app/book` for authenticated users (no change).
- The Login page can link to `/book` as a guest entry point (optional, out of scope for this spec).

---

## Feature 2: Welcome Banner on Home

### Goal

Show a persistent welcome banner at the top of the client home page immediately after login, indicating the user's role.

### Placement

Inserted between the existing header block and the "Agendar Agora" CTA button in `ClientHome`.

### Content

```
Seja bem-vindo ao BarberAgenda!   [Barbeiro]
                                  [Cliente]
```

- Static text: *"Seja bem-vindo ao BarberAgenda!"*
- Role badge: reads `role` from `authStore`
  - `role === 'admin'` → badge text **"Barbeiro"**, color `accent` (gold)
  - `role === 'client'` → badge text **"Cliente"**, color secondary (muted/blue)

### Implementation

A single JSX block (~15 lines) added directly inside `ClientHome`. No new files, no state, no hooks. Reads `role` from the existing `useAuthStore` selector already used in the component.

Uses the existing `Badge` component pattern from `src/components/ui/Badge.tsx` for the role pill.

### Visibility

Always visible when on the home page. No dismiss button, no sessionStorage logic.

---

## Files to Change

| File | Change |
|---|---|
| `frontend/src/router/index.tsx` | Move `book` outside `PrivateRoute`; add public `/book` route |
| `frontend/src/pages/client/Book.tsx` | Add auth check on step 2; render login modal if not authenticated |
| `frontend/src/pages/client/Home.tsx` | Add welcome banner block between header and CTA |
| `src/BarberShop.Api/Controllers/AppointmentController.cs` | Verify/add `[AllowAnonymous]` on `GetAvailableSlots` endpoint |

---

## Out of Scope

- Saving draft appointment state across login redirect (nice-to-have, not required)
- Showing the banner on other pages besides Home
- Changing the Login page to link to `/book`
