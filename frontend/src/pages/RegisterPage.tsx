import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Link, useNavigate } from 'react-router-dom';
import { Form, Input, Button, Typography, Row, Col, message, Space } from 'antd';
import { UserOutlined, MailOutlined, LockOutlined } from '@ant-design/icons';
import { useAuth } from '../services/AuthContext';
import { useState } from 'react';

const { Title, Text, Paragraph } = Typography;

const registerSchema = z.object({
  firstName: z.string().min(2, 'At least 2 characters'),
  lastName: z.string().min(2, 'At least 2 characters'),
  email: z.string().email('Enter a valid email'),
  password: z.string().min(8, 'At least 8 characters'),
  confirmPassword: z.string(),
}).refine((d) => d.password === d.confirmPassword, {
  message: "Passwords don't match",
  path: ['confirmPassword'],
});

type RegisterForm = z.infer<typeof registerSchema>;

export function RegisterPage() {
  const { register: registerUser } = useAuth();
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);

  const { control, handleSubmit, formState: { errors } } = useForm<RegisterForm>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (data: RegisterForm) => {
    setIsLoading(true);
    try {
      await registerUser(data.firstName, data.lastName, data.email, data.password);
      message.success('Account created! Welcome to InterviewAI 🎉');
      navigate('/dashboard');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        || 'Registration failed.';
      message.error(msg);
    } finally {
      setIsLoading(false);
    }
  };

  const inputStyle = {
    background: 'rgba(255,255,255,0.05)',
    border: '1px solid rgba(255,255,255,0.1)',
    borderRadius: 10,
    color: '#fff',
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
          Create your account
        </Title>
        <Paragraph style={{ color: 'rgba(255,255,255,0.5)', margin: 0 }}>
          Start your interview preparation journey
        </Paragraph>
      </Space>

      <Form layout="vertical" onFinish={handleSubmit(onSubmit)} size="large">
        <Row gutter={16}>
          <Col span={12}>
            <Form.Item
              label={<Text style={{ color: 'rgba(255,255,255,0.7)' }}>First Name</Text>}
              validateStatus={errors.firstName ? 'error' : ''}
              help={errors.firstName?.message}
            >
              <Controller
                name="firstName"
                control={control}
                defaultValue=""
                render={({ field }) => (
                  <Input {...field} id="reg-firstname" prefix={<UserOutlined style={{ color: '#6C63FF' }} />}
                    placeholder="John" style={inputStyle} />
                )}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              label={<Text style={{ color: 'rgba(255,255,255,0.7)' }}>Last Name</Text>}
              validateStatus={errors.lastName ? 'error' : ''}
              help={errors.lastName?.message}
            >
              <Controller
                name="lastName"
                control={control}
                defaultValue=""
                render={({ field }) => (
                  <Input {...field} id="reg-lastname" placeholder="Doe" style={inputStyle} />
                )}
              />
            </Form.Item>
          </Col>
        </Row>

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
              <Input {...field} id="reg-email" prefix={<MailOutlined style={{ color: '#6C63FF' }} />}
                placeholder="john@example.com" style={inputStyle} />
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
              <Input.Password {...field} id="reg-password"
                prefix={<LockOutlined style={{ color: '#6C63FF' }} />}
                placeholder="Min. 8 characters" style={inputStyle} />
            )}
          />
        </Form.Item>

        <Form.Item
          label={<Text style={{ color: 'rgba(255,255,255,0.7)' }}>Confirm Password</Text>}
          validateStatus={errors.confirmPassword ? 'error' : ''}
          help={errors.confirmPassword?.message}
        >
          <Controller
            name="confirmPassword"
            control={control}
            defaultValue=""
            render={({ field }) => (
              <Input.Password {...field} id="reg-confirm-password"
                prefix={<LockOutlined style={{ color: '#6C63FF' }} />}
                placeholder="Repeat password" style={inputStyle} />
            )}
          />
        </Form.Item>

        <Button
          id="register-submit"
          type="primary"
          htmlType="submit"
          loading={isLoading}
          block
          style={{
            height: 48, fontSize: 15, fontWeight: 600,
            background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
            border: 'none', borderRadius: 10,
            boxShadow: '0 4px 24px rgba(108,99,255,0.35)',
            marginTop: 8,
          }}
        >
          Create Account
        </Button>
      </Form>

      <Text style={{ color: 'rgba(255,255,255,0.5)', display: 'block', textAlign: 'center', marginTop: 24 }}>
        Already have an account?{' '}
        <Link to="/login" style={{ color: '#6C63FF', fontWeight: 600 }}>Sign in</Link>
      </Text>
    </div>
  );
}
