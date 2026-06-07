import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, useNavigate } from 'react-router-dom';
import { Form, Input, Button, Typography, Divider, message, Space } from 'antd';
import { MailOutlined, LockOutlined, EyeInvisibleOutlined, EyeTwoTone } from '@ant-design/icons';
import { useAuth } from '../services/AuthContext';
import { useState } from 'react';

const { Title, Text, Paragraph } = Typography;

const loginSchema = z.object({
  email: z.string().email('Enter a valid email'),
  password: z.string().min(1, 'Password is required'),
});

type LoginForm = z.infer<typeof loginSchema>;

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);

  const { control, handleSubmit, formState: { errors } } = useForm<LoginForm>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginForm) => {
    setIsLoading(true);
    try {
      await login(data.email, data.password);
      message.success('Welcome back!');
      navigate('/dashboard');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        || 'Login failed. Please check your credentials.';
      message.error(msg);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div style={{
      background: 'rgba(255,255,255,0.03)',
      backdropFilter: 'blur(40px)',
      border: '1px solid rgba(255,255,255,0.08)',
      borderRadius: 24,
      padding: '40px 40px 32px',
    }}>
      <Space direction="vertical" size={4} style={{ width: '100%', marginBottom: 32 }}>
        <Title level={2} style={{ color: '#fff', margin: 0, fontWeight: 700 }}>
          Welcome back
        </Title>
        <Paragraph style={{ color: 'rgba(255,255,255,0.5)', margin: 0 }}>
          Sign in to continue your interview prep
        </Paragraph>
      </Space>

      <Form layout="vertical" onFinish={handleSubmit(onSubmit)} size="large">
        <Form.Item
          label={<Text style={{ color: 'rgba(255,255,255,0.7)' }}>Email</Text>}
          validateStatus={errors.email ? 'error' : ''}
          help={errors.email?.message}
        >
          <Controller
            name="email"
            control={control}
            defaultValue=""
            render={({ field }) => (
              <Input
                {...field}
                id="login-email"
                prefix={<MailOutlined style={{ color: '#6C63FF' }} />}
                placeholder="you@example.com"
                style={{
                  background: 'rgba(255,255,255,0.05)',
                  border: '1px solid rgba(255,255,255,0.1)',
                  borderRadius: 10, color: '#fff',
                }}
              />
            )}
          />
        </Form.Item>

        <Form.Item
          label={<Text style={{ color: 'rgba(255,255,255,0.7)' }}>Password</Text>}
          validateStatus={errors.password ? 'error' : ''}
          help={errors.password?.message}
        >
          <Controller
            name="password"
            control={control}
            defaultValue=""
            render={({ field }) => (
              <Input.Password
                {...field}
                id="login-password"
                prefix={<LockOutlined style={{ color: '#6C63FF' }} />}
                placeholder="••••••••"
                iconRender={(visible) => visible ? <EyeTwoTone /> : <EyeInvisibleOutlined />}
                style={{
                  background: 'rgba(255,255,255,0.05)',
                  border: '1px solid rgba(255,255,255,0.1)',
                  borderRadius: 10, color: '#fff',
                }}
              />
            )}
          />
        </Form.Item>

        <div style={{ textAlign: 'right', marginTop: -12, marginBottom: 24 }}>
          <Link to="/forgot-password" style={{ color: '#6C63FF', fontSize: 13 }}>
            Forgot password?
          </Link>
        </div>

        <Button
          id="login-submit"
          type="primary"
          htmlType="submit"
          loading={isLoading}
          block
          style={{
            height: 48, fontSize: 15, fontWeight: 600,
            background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
            border: 'none', borderRadius: 10,
            boxShadow: '0 4px 24px rgba(108,99,255,0.35)',
          }}
        >
          Sign In
        </Button>
      </Form>

      <Divider style={{ borderColor: 'rgba(255,255,255,0.08)', color: 'rgba(255,255,255,0.3)' }}>
        or
      </Divider>

      <Text style={{ color: 'rgba(255,255,255,0.5)', display: 'block', textAlign: 'center' }}>
        Don't have an account?{' '}
        <Link to="/register" style={{ color: '#6C63FF', fontWeight: 600 }}>
          Sign up for free
        </Link>
      </Text>
    </div>
  );
}
