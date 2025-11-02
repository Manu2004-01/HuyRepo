-- SQL Script to create payments table for Supabase (Fixed to match DbContext)
-- Run this script in Supabase SQL Editor

create table if not exists public.payments (
    payment_id serial primary key,
    user_id int not null references public.users(user_id) on delete restrict,
    order_code bigint not null unique,
    payment_link_id text default '',
    amount int not null,
    description text default '',
    status text default '',
    payment_type text default 'Premium',
    premium_expiry_date timestamp with time zone,
    created_at timestamp with time zone default now(),
    paid_at timestamp with time zone
);

-- Create indexes for better query performance
create index if not exists idx_payments_user_id on public.payments(user_id);
create index if not exists idx_payments_order_code on public.payments(order_code);
create index if not exists idx_payments_status on public.payments(status);
create index if not exists idx_payments_premium_expiry on public.payments(premium_expiry_date) 
    where premium_expiry_date is not null;

