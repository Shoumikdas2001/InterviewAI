import { useNavigate } from 'react-router-dom';
import { Typography, Button, Row, Col, Card, Statistic, Space } from 'antd';
import {
  ThunderboltOutlined, SafetyCertificateOutlined, RocketOutlined,
  ArrowRightOutlined, PlayCircleOutlined,
} from '@ant-design/icons';

const { Title, Paragraph, Text } = Typography;

const features = [
  {
    icon: <ThunderboltOutlined style={{ fontSize: 32, color: '#6C63FF' }} />,
    title: 'AI-Powered Questions',
    desc: 'Gemini generates personalized questions based on your resume and target role.',
  },
  {
    icon: <SafetyCertificateOutlined style={{ fontSize: 32, color: '#48c6ef' }} />,
    title: 'ATS Score Analysis',
    desc: 'Get instant ATS score with actionable recommendations to improve your resume.',
  },
  {
    icon: <RocketOutlined style={{ fontSize: 32, color: '#F59E0B' }} />,
    title: 'Learning Roadmap',
    desc: 'AI-generated personalized study plans to close skill gaps for your target role.',
  },
];

const stats = [
  { value: '10K+', label: 'Interviews Conducted' },
  { value: '94%', label: 'User Satisfaction' },
  { value: '3x', label: 'Faster Interview Prep' },
  { value: '50+', label: 'Job Roles Supported' },
];

export function LandingPage() {
  const navigate = useNavigate();

  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #0f0c29 0%, #302b63 50%, #24243e 100%)',
      color: '#fff',
    }}>
      {/* Navbar */}
      <nav style={{
        padding: '16px 48px',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        borderBottom: '1px solid rgba(255,255,255,0.07)',
        position: 'sticky',
        top: 0,
        background: 'rgba(15,12,41,0.85)',
        backdropFilter: 'blur(20px)',
        zIndex: 100,
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <div style={{
            width: 36, height: 36,
            background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
            borderRadius: 10,
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: 16, color: '#fff', fontWeight: 800,
          }}>AI</div>
          <Text style={{ color: '#fff', fontWeight: 700, fontSize: 20 }}>InterviewAI</Text>
        </div>

        <Space>
          <Button type="text" style={{ color: 'rgba(255,255,255,0.7)' }} onClick={() => navigate('/login')}>
            Sign In
          </Button>
          <Button
            type="primary"
            size="large"
            style={{ background: 'linear-gradient(135deg, #6C63FF, #48c6ef)', border: 'none', borderRadius: 8 }}
            onClick={() => navigate('/register')}
          >
            Start Free
          </Button>
        </Space>
      </nav>

      {/* Hero */}
      <section style={{ padding: '100px 48px 80px', textAlign: 'center', position: 'relative' }}>
        {/* Decorative blobs */}
        <div style={{
          position: 'absolute', top: '10%', left: '5%',
          width: 500, height: 500, borderRadius: '50%',
          background: 'radial-gradient(circle, rgba(108,99,255,0.2) 0%, transparent 70%)',
          filter: 'blur(60px)', pointerEvents: 'none',
        }} />
        <div style={{
          position: 'absolute', top: '20%', right: '5%',
          width: 400, height: 400, borderRadius: '50%',
          background: 'radial-gradient(circle, rgba(72,198,239,0.15) 0%, transparent 70%)',
          filter: 'blur(60px)', pointerEvents: 'none',
        }} />

        <div style={{ position: 'relative' }}>
          <div style={{
            display: 'inline-block',
            padding: '6px 16px',
            background: 'rgba(108,99,255,0.15)',
            border: '1px solid rgba(108,99,255,0.3)',
            borderRadius: 20,
            marginBottom: 24,
            fontSize: 13,
            color: '#a78bfa',
            fontWeight: 500,
          }}>
            🚀 Powered by Google Gemini AI
          </div>

          <Title
            level={1}
            style={{
              color: '#fff',
              fontSize: 'clamp(40px, 6vw, 72px)',
              fontWeight: 800,
              lineHeight: 1.1,
              marginBottom: 24,
              letterSpacing: '-1px',
            }}
          >
            Ace Your Next{' '}
            <span style={{
              background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
            }}>
              Technical Interview
            </span>
          </Title>

          <Paragraph style={{
            color: 'rgba(255,255,255,0.6)',
            fontSize: 20,
            maxWidth: 600,
            margin: '0 auto 48px',
            lineHeight: 1.7,
          }}>
            AI-powered mock interviews, ATS resume analysis, skill gap detection,
            and personalized learning roadmaps — all in one platform.
          </Paragraph>

          <Space size={16} wrap style={{ justifyContent: 'center' }}>
            <Button
              type="primary"
              size="large"
              icon={<PlayCircleOutlined />}
              style={{
                height: 54,
                padding: '0 32px',
                fontSize: 16,
                fontWeight: 600,
                background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
                border: 'none',
                borderRadius: 12,
                boxShadow: '0 8px 32px rgba(108,99,255,0.4)',
              }}
              onClick={() => navigate('/register')}
            >
              Start for Free
            </Button>
            <Button
              size="large"
              icon={<ArrowRightOutlined />}
              style={{
                height: 54,
                padding: '0 32px',
                fontSize: 16,
                background: 'rgba(255,255,255,0.06)',
                border: '1px solid rgba(255,255,255,0.12)',
                color: '#fff',
                borderRadius: 12,
              }}
              onClick={() => navigate('/login')}
            >
              Sign In
            </Button>
          </Space>
        </div>
      </section>

      {/* Stats */}
      <section style={{
        padding: '40px 48px',
        background: 'rgba(255,255,255,0.02)',
        borderTop: '1px solid rgba(255,255,255,0.06)',
        borderBottom: '1px solid rgba(255,255,255,0.06)',
      }}>
        <Row gutter={32} justify="center">
          {stats.map((stat, i) => (
            <Col key={i} xs={12} sm={6}>
              <Statistic
                value={stat.value}
                title={
                  <Text style={{ color: 'rgba(255,255,255,0.5)', fontSize: 13 }}>{stat.label}</Text>
                }
                valueStyle={{ color: '#fff', fontWeight: 700, fontSize: 32, textAlign: 'center' }}
                style={{ textAlign: 'center' }}
              />
            </Col>
          ))}
        </Row>
      </section>

      {/* Features */}
      <section style={{ padding: '80px 48px' }}>
        <Title level={2} style={{ color: '#fff', textAlign: 'center', marginBottom: 48, fontWeight: 700 }}>
          Everything you need to land your{' '}
          <span style={{ color: '#6C63FF' }}>dream job</span>
        </Title>
        <Row gutter={[32, 32]} justify="center">
          {features.map((feat, i) => (
            <Col key={i} xs={24} sm={12} md={8}>
              <Card
                style={{
                  background: 'rgba(255,255,255,0.03)',
                  border: '1px solid rgba(255,255,255,0.08)',
                  borderRadius: 20,
                  height: '100%',
                  transition: 'all 0.3s ease',
                }}
                className="feature-card"
              >
                <Space direction="vertical" size={16}>
                  <div style={{
                    width: 60, height: 60,
                    background: 'rgba(108,99,255,0.1)',
                    borderRadius: 16,
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                  }}>
                    {feat.icon}
                  </div>
                  <Title level={4} style={{ color: '#fff', margin: 0 }}>{feat.title}</Title>
                  <Paragraph style={{ color: 'rgba(255,255,255,0.6)', margin: 0 }}>
                    {feat.desc}
                  </Paragraph>
                </Space>
              </Card>
            </Col>
          ))}
        </Row>
      </section>

      {/* CTA */}
      <section style={{ padding: '80px 48px', textAlign: 'center' }}>
        <div style={{
          background: 'linear-gradient(135deg, rgba(108,99,255,0.15), rgba(72,198,239,0.1))',
          border: '1px solid rgba(108,99,255,0.2)',
          borderRadius: 24,
          padding: '60px 40px',
          maxWidth: 700,
          margin: '0 auto',
        }}>
          <Title level={2} style={{ color: '#fff', margin: '0 0 16px' }}>
            Ready to level up your interview skills?
          </Title>
          <Paragraph style={{ color: 'rgba(255,255,255,0.6)', fontSize: 16, marginBottom: 32 }}>
            Join thousands of developers who use InterviewAI to land their dream jobs.
          </Paragraph>
          <Button
            type="primary"
            size="large"
            style={{
              height: 54, padding: '0 40px', fontSize: 16, fontWeight: 600,
              background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
              border: 'none', borderRadius: 12,
            }}
            onClick={() => navigate('/register')}
          >
            Get Started Free →
          </Button>
        </div>
      </section>

      {/* Footer */}
      <footer style={{
        padding: '24px 48px',
        borderTop: '1px solid rgba(255,255,255,0.06)',
        textAlign: 'center',
      }}>
        <Text style={{ color: 'rgba(255,255,255,0.3)', fontSize: 13 }}>
          © 2024 InterviewAI. Built with ASP.NET Core 8 + React 19 + Google Gemini.
        </Text>
      </footer>
    </div>
  );
}
