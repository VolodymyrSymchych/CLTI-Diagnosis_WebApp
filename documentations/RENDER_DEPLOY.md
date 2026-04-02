# Deploy To Render (Free Plan)

## 1. Push repository
Push this branch to GitHub.

## 2. Create service on Render
1. Open Render dashboard.
2. `New +` -> `Blueprint`.
3. Select this repository.
4. Render will detect [`render.yaml`](/Users/symchychpc/RiderProjects/CLTI-Diagnosis_WebApp/render.yaml).

## 3. Configure required environment variables
Set these in Render (Environment tab):

1. `DATABASE_URL`  
Use your Neon URL, for example:
`postgresql://.../clti?sslmode=require&channel_binding=require`
2. `Jwt__Key`  
Use a long random secret (at least 32+ chars).

Already predefined by blueprint:
1. `ASPNETCORE_ENVIRONMENT=Production`
2. `PORT=10000`
3. `Jwt__Issuer=CLTI.Diagnosis`
4. `Jwt__Audience=CLTI.Diagnosis.Client`

## 4. Deploy
Start deploy in Render UI.  
App should become available on your Render URL.

## 5. Health check
Render uses:
`/health`

If health check is green but login fails:
1. verify `DATABASE_URL`
2. verify `Jwt__Key`
3. check logs for `SessionTokenMiddleware` / auth warnings
