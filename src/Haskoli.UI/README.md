# Haskoli.UI

Cliente Angular (TypeScript) de Haskoli. Es la arquitectura front inicial.

Conecta con `Haskoli.Api` (`https://localhost:9100` en el perfil HTTPS).

## Requisitos

- Node.js `^22.22.3` o `^24.15.0` (Angular 22).
- API en ejecución: `dotnet run --project src/Haskoli.Api/Haskoli.Api.csproj`

## Desarrollo

```bash
cd src/Haskoli.UI
npm start
```

Abre `http://localhost:4200/`. En desarrollo, `/api` se reenvía a `https://localhost:9100` mediante `proxy.conf.json`.

## Arquitectura

```
src/app/
  core/        HTTP, JWT, modelos del contrato de la API
  features/    módulos de negocio (vacío a propósito)
  shared/      UI reutilizable (vacío a propósito)
```

Las llamadas HTTP deben pasar por `ApiClient`. El interceptor adjunta el Bearer token guardado en `AuthTokenStore` cuando exista.

## Build

```bash
npm run build
```
