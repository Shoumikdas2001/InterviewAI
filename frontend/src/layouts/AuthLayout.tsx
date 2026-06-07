import { Outlet, useNavigate } from 'react-router-dom';
import { Button, Typography } from 'antd';

const { Text } = Typography;

export function AuthLayout() {
  const navigate = useNavigate();

  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #0f0c29 0%, #302b63 50%, #24243e 100%)',
      display: 'flex',
      flexDirection: 'column',
    }}>
      {/* Navbar */}
      <nav style={{
        padding: '16px 48px',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        borderBottom: '1px solid rgba(255,255,255,0.06)',
      }}>
        <div
          style={{ display: 'flex', alignItems: 'center', gap: 10, cursor: 'pointer' }}
          onClick={() => navigate('/')}
        >
          <div style={{
            width: 32, height: 32,
            background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
            borderRadius: 8,
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: 14, color: '#fff', fontWeight: 800,
          }}>AI</div>
          <Text style={{ color: '#fff', fontWeight: 700, fontSize: 18 }}>InterviewAI</Text>
        </div>

        <div style={{ display: 'flex', gap: 12 }}>
          <Button type="text" style={{ color: 'rgba(255,255,255,0.7)' }} onClick={() => navigate('/login')}>
            Sign In
          </Button>
          <Button
            type="primary"
            style={{ background: 'linear-gradient(135deg, #6C63FF, #48c6ef)', border: 'none' }}
            onClick={() => navigate('/register')}
          >
            Get Started
          </Button>
        </div>
      </nav>

      {/* Content */}
      <div style={{
        flex: 1,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: '48px 24px',
      }}>
        {/* Decorative blobs */}
        <div style={{
          position: 'fixed', top: '20%', left: '10%',
          width: 400, height: 400, borderRadius: '50%',
          background: 'radial-gradient(circle, rgba(108,99,255,0.15) 0%, transparent 70%)',
          pointerEvents: 'none',
        }} />
        <div style={{
          position: 'fixed', bottom: '20%', right: '10%',
          width: 300, height: 300, borderRadius: '50%',
          background: 'radial-gradient(circle, rgba(72,198,239,0.12) 0%, transparent 70%)',
          pointerEvents: 'none',
        }} />

        <div style={{ width: '100%', maxWidth: 440, position: 'relative', zIndex: 1 }}>
          <Outlet />
        </div>
      </div>
    </div>
  );
}
