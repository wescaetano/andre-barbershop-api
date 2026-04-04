# Fluxo Pagamento Checkout Pro

Vou explicar **todo o fluxo do sistema desde o login até o pagamento aprovado no Mercado Pago**, considerando a arquitetura que montamos (**Clean Architecture + Checkout Pro**).

Vou mostrar também **o que acontece no frontend, backend e Mercado Pago** em cada etapa.

---

# 🔐 1. Usuário entra no sistema

Fluxo:

```
Usuário → Frontend → API
```

### Passos

1️⃣ Usuário abre o sistema

2️⃣ Faz login

Endpoint:

```
POST /auth/login
```

Payload:

```
{
  "email":"cliente@email.com",
  "password":"123456"
}
```

Backend:

1. Controller recebe requisição
2. AuthService valida usuário
3. senha é comparada com **hash**
4. JWT é gerado

Resposta:

```
{
  "token":"JWT_TOKEN"
}
```

Esse token será usado nas próximas requisições.

---

# 📅 2. Usuário busca horários disponíveis

Agora o cliente quer marcar o corte.

Endpoint:

```
GET /appointments/available?date=2026-03-15
```

Fluxo interno:

```
Controller
   ↓
AppointmentService
   ↓
AppointmentRepository
   ↓
Database
```

Lógica:

1️⃣ sistema gera horários de 30 minutos

Exemplo:

```
09:00
09:30
10:00
10:30
11:00
```

2️⃣ consulta agendamentos existentes

```
SELECT StartTime
FROM Appointments
WHEREDATE(StartTime)='2026-03-15'
```

3️⃣ remove horários ocupados

Resposta:

```
[
"09:00",
"09:30",
"11:00",
"11:30"
]
```

---

# ✂️ 3. Usuário cria o agendamento

Usuário escolhe horário.

Endpoint:

```
POST /appointments
```

Payload:

```
{
 "date":"2026-03-15",
 "startTime":"10:00"
}
```

Backend cria:

```
Appointment
```

Registro no banco:

```
Id: 1
UserId: 10
StartTime: 10:00
EndTime: 10:30
Status: WaitingPayment
```

Status inicial:

```
WaitingPayment
```

Porque o pagamento ainda não foi feito.

---

# 💳 4. Criar pagamento no Mercado Pago

Agora o frontend chama:

```
POST /payments/create/{appointmentId}
```

Exemplo:

```
POST /payments/create/1
```

Fluxo:

```
Controller
   ↓
PaymentService
   ↓
MercadoPagoService
   ↓
MercadoPago SDK
```

---

# 🧾 5. Backend cria Payment Preference

O sistema cria uma **preferência de pagamento** no Mercado Pago.

Exemplo de dados enviados:

```
{
 "items": [
  {
   "title":"Corte de cabelo",
   "quantity":1,
   "currency_id":"BRL",
   "unit_price":30
  }
 ],
 "external_reference":"appointment_1",
 "notification_url":"https://meusite.com/payments/webhook"
}
```

Campos importantes:

| Campo | Função |
| --- | --- |
| items | produto |
| external_reference | id do agendamento |
| notification_url | webhook |

---

# 🔗 6. Mercado Pago retorna URL de pagamento

Resposta da API do Mercado Pago:

```
{
 "init_point":"https://www.mercadopago.com.br/checkout/v1/..."
}
```

Backend retorna para o frontend:

```
{
 "checkoutUrl":"https://www.mercadopago.com.br/checkout/v1/..."
}
```

---

# 🌐 7. Usuário é redirecionado ao checkout

Frontend redireciona:

```
window.location.href=checkoutUrl
```

Usuário vê página do Mercado Pago.

Exemplo:

```
Pagar R$30
Cartão
Pix
Saldo Mercado Pago
```

---

# 💰 8. Usuário realiza pagamento

Agora o processo ocorre **dentro do Mercado Pago**.

Fluxo:

```
Cliente paga
↓
Mercado Pago processa
↓
Mercado Pago confirma pagamento
```

Status possíveis:

```
approved
pending
rejected
```

---

# 🔔 9. Mercado Pago envia Webhook

Depois do pagamento, o Mercado Pago chama seu backend.

Endpoint configurado:

```
POST /payments/webhook
```

Payload simplificado:

```
{
 "type":"payment",
 "data": {
  "id":"123456789"
 }
}
```

---

# 🔎 10. Backend consulta pagamento

Boa prática:

⚠️ nunca confiar apenas no webhook.

Backend consulta:

```
GET https://api.mercadopago.com/v1/payments/{id}
```

Resposta:

```
{
 "id":123456,
 "status":"approved",
 "transaction_amount":30,
 "external_reference":"appointment_1"
}
```

---

# 🧠 11. Backend atualiza banco

PaymentService executa:

1️⃣ encontra pagamento

2️⃣ encontra agendamento

3️⃣ atualiza status

Tabela Payments:

```
PaymentId: 123456
Status: Approved
Amount: 30
```

Tabela Appointment:

```
Status: Paid
```

---

# 📜 12. Histórico de pagamentos

Usuário pode ver pagamentos.

Endpoint:

```
GET /payments/history
```

Resposta:

```
[
 {
  "date":"2026-03-15",
  "service":"Corte de cabelo",
  "amount":30,
  "status":"Paid"
 }
]
```

Isso serve como **comprovante para o barbeiro**.

---

# 🧔 13. Barbeiro consulta agenda

Painel do barbeiro:

```
GET /barber/appointments/today
```

Retorno:

```
[
 {
  "cliente":"João",
  "horario":"10:00",
  "status":"Pago"
 }
]
```

---

# 🔄 Fluxo COMPLETO resumido

```
1 Usuário faz login
        ↓
2 Busca horários disponíveis
        ↓
3 Cria agendamento
        ↓
4 Backend cria pagamento no Mercado Pago
        ↓
5 Frontend redireciona para checkout
        ↓
6 Usuário paga
        ↓
7 Mercado Pago envia webhook
        ↓
8 Backend consulta API Mercado Pago
        ↓
9 Backend atualiza pagamento
        ↓
10 Agendamento vira "Paid"
```

---

# ⚠️ Validações importantes

### Evitar agendamento sem pagamento

Se pagamento não acontecer:

```
WaitingPayment
```

Pode cancelar automaticamente após:

```
10 minutos
```

---

### Evitar duplicação de pagamento

Use:

```
external_reference
```

---

### Evitar fraude

Sempre confirmar pagamento via API.

---

# 🚀 Arquitetura do fluxo de pagamento

Frontend
   ↓
API Controller
   ↓
PaymentService
   ↓
MercadoPagoService
   ↓
MercadoPago API
   ↓
Checkout Page
   ↓
Pagamento
   ↓
Webhook
   ↓
PaymentService
   ↓
Database