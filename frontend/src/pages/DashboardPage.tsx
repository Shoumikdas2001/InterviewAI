import { useEffect, useState } from 'react';
import {
  Row, Col, Card, Statistic, Typography, Tag, Table, Progress,
  Spin, Empty, Button, Tooltip, Space
} from 'antd';
import {
  TrophyOutlined, RiseOutlined, ClockCircleOutlined, FireOutlined,
  ArrowRightOutlined, PlayCircleOutlined,
} from '@ant-design/icons';
import {
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip as RechartsTooltip,
  ResponsiveContainer, RadarChart, Radar, PolarGrid, PolarAngleAxis,
  PolarRadiusAxis,
} from 'recharts';
import { dashboardApi } from '../api/interview.api';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../services/AuthContext';
import type { Dashboard } from '../types';

const { Title, Text, Paragraph } = Typography;

const cardStyle = {
  background: 'rgba(255,255,255,0.03)',
  border: '1px solid rgba(255,255,255,0.07)',
  borderRadius: 16,
};

function StatCard({ title, value, icon, color, suffix }: {
  title: string; value: string | number; icon: React.ReactNode;
  color: string; suffix?: string;
}) {
  return (
    <Card style={{
      ...cardStyle,
      borderLeft: `3px solid ${color}`,
    }}>
      <Space align="center" style={{ width: '100%', justifyContent: 'space-between' }}>
        <Statistic
          title={<Text style={{ color: 'rgba(255,255,255,0.5)', fontSize: 12 }}>{title}</Text>}
          value={value}
          suffix={suffix}
          valueStyle={{ color: '#fff', fontWeight: 700, fontSize: 28 }}
        />
        <div style={{
          width: 48, height: 48, borderRadius: 12,
          background: `${color}20`,
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          fontSize: 20, color,
        }}>
          {icon}
        </div>
      </Space>
    </Card>
  );
}

export function DashboardPage() {
  const [data, setData] = useState<Dashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();
  const { user } = useAuth();

  useEffect(() => {
    dashboardApi.getAnalytics()
      .then(setData)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  if (loading) return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
      <Spin size="large" />
    </div>
  );

  if (!data || data.totalInterviews === 0) {
    return (
      <div>
        <Title level={3} style={{ color: '#fff', marginBottom: 32 }}>
          👋 Welcome, {user?.firstName}!
        </Title>
        <Card style={{
          ...cardStyle,
          textAlign: 'center',
          padding: '60px 40px',
        }}>
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description={<Text style={{ color: 'rgba(255,255,255,0.5)' }}>No interviews yet</Text>}
          />
          <Paragraph style={{ color: 'rgba(255,255,255,0.5)', marginBottom: 32 }}>
            Start your first AI-powered mock interview to see your analytics here.
          </Paragraph>
          <Space wrap style={{ justifyContent: 'center' }}>
            <Button
              type="primary"
              icon={<PlayCircleOutlined />}
              size="large"
              style={{
                background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
                border: 'none', borderRadius: 10,
              }}
              onClick={() => navigate('/interview/new')}
            >
              Start First Interview
            </Button>
            <Button
              size="large"
              style={{ borderRadius: 10 }}
              onClick={() => navigate('/resume')}
            >
              Upload Resume First
            </Button>
          </Space>
        </Card>
      </div>
    );
  }

  const radarData = data.skillPerformance.slice(0, 6).map((s) => ({
    skill: s.skill.length > 12 ? s.skill.substring(0, 12) + '...' : s.skill,
    score: s.score,
  }));

  const historyColumns = [
    {
      title: 'Interview',
      dataIndex: 'jobRole',
      key: 'jobRole',
      render: (text: string, rec: { jobRole: string; title: string }) => (
        <div>
          <Text style={{ color: '#fff', fontWeight: 600, display: 'block' }}>{rec.title}</Text>
          <Text style={{ color: 'rgba(255,255,255,0.4)', fontSize: 12 }}>{text}</Text>
        </div>
      ),
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: string) => (
        <Tag color={status === 'Completed' ? 'green' : status === 'InProgress' ? 'blue' : 'default'}>
          {status}
        </Tag>
      ),
    },
    {
      title: 'Score',
      dataIndex: 'overallScore',
      key: 'score',
      render: (score: number | undefined) => score !== undefined ? (
        <Text style={{
          color: score >= 7 ? '#10B981' : score >= 5 ? '#F59E0B' : '#EF4444',
          fontWeight: 700, fontSize: 16,
        }}>
          {score?.toFixed(1)}/10
        </Text>
      ) : <Text style={{ color: 'rgba(255,255,255,0.3)' }}>—</Text>,
    },
    {
      title: '',
      key: 'action',
      render: (_: unknown, rec: { id: string }) => (
        <Button
          type="text"
          icon={<ArrowRightOutlined />}
          style={{ color: '#6C63FF' }}
          onClick={() => navigate(`/interview/${rec.id}/results`)}
        />
      ),
    },
  ];

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 32 }}>
        <div>
          <Title level={3} style={{ color: '#fff', margin: 0 }}>
            Welcome back, {user?.firstName} 👋
          </Title>
          <Text style={{ color: 'rgba(255,255,255,0.5)' }}>
            Here's your interview performance overview
          </Text>
        </div>
        <Button
          type="primary"
          icon={<PlayCircleOutlined />}
          size="large"
          style={{
            background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
            border: 'none', borderRadius: 10,
          }}
          onClick={() => navigate('/interview/new')}
        >
          New Interview
        </Button>
      </div>

      {/* KPI Cards */}
      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12} lg={6}>
          <StatCard title="Total Interviews" value={data.totalInterviews}
            icon={<TrophyOutlined />} color="#6C63FF" />
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <StatCard title="Avg. Score" value={data.averageScore.toFixed(1)}
            suffix="/10" icon={<TrophyOutlined />} color="#10B981" />
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <StatCard title="Improvement"
            value={`${data.improvementPercentage > 0 ? '+' : ''}${data.improvementPercentage}%`}
            icon={<RiseOutlined />} color="#F59E0B" />
        </Col>
        <Col xs={24} sm={12} lg={6}>
          <StatCard title="Practice Hours" value={data.totalPracticeHours.toFixed(1)}
            suffix="hrs" icon={<ClockCircleOutlined />} color="#48c6ef" />
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        {/* Score Trend */}
        <Col xs={24} lg={14}>
          <Card title={<Text style={{ color: '#fff', fontWeight: 600 }}>Score Trend</Text>}
            style={cardStyle}>
            {data.scoreTrend.length > 0 ? (
              <ResponsiveContainer width="100%" height={250}>
                <LineChart data={data.scoreTrend}>
                  <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.05)" />
                  <XAxis dataKey="date" stroke="rgba(255,255,255,0.3)" tick={{ fontSize: 11 }} />
                  <YAxis domain={[0, 10]} stroke="rgba(255,255,255,0.3)" tick={{ fontSize: 11 }} />
                  <RechartsTooltip
                    contentStyle={{ background: '#1a1650', border: '1px solid rgba(108,99,255,0.3)', borderRadius: 8 }}
                    labelStyle={{ color: '#fff' }}
                  />
                  <Line
                    type="monotone" dataKey="score" stroke="#6C63FF"
                    strokeWidth={2} dot={{ fill: '#6C63FF', r: 4 }}
                    activeDot={{ r: 6, fill: '#48c6ef' }}
                  />
                </LineChart>
              </ResponsiveContainer>
            ) : (
              <Empty description={<Text style={{ color: 'rgba(255,255,255,0.4)' }}>Complete interviews to see trend</Text>} />
            )}
          </Card>
        </Col>

        {/* Skill Radar */}
        <Col xs={24} lg={10}>
          <Card title={<Text style={{ color: '#fff', fontWeight: 600 }}>Skill Performance</Text>}
            style={cardStyle}>
            {radarData.length > 0 ? (
              <ResponsiveContainer width="100%" height={250}>
                <RadarChart data={radarData}>
                  <PolarGrid stroke="rgba(255,255,255,0.1)" />
                  <PolarAngleAxis dataKey="skill" tick={{ fill: 'rgba(255,255,255,0.6)', fontSize: 11 }} />
                  <PolarRadiusAxis domain={[0, 100]} tick={{ fill: 'rgba(255,255,255,0.3)', fontSize: 9 }} />
                  <Radar name="Score" dataKey="score" stroke="#6C63FF" fill="#6C63FF" fillOpacity={0.25} />
                </RadarChart>
              </ResponsiveContainer>
            ) : (
              <Empty description={<Text style={{ color: 'rgba(255,255,255,0.4)' }}>Complete an interview first</Text>} />
            )}
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]}>
        {/* Recent Interviews */}
        <Col xs={24} lg={14}>
          <Card
            title={<Text style={{ color: '#fff', fontWeight: 600 }}>Recent Interviews</Text>}
            extra={<Button type="link" style={{ color: '#6C63FF' }} onClick={() => navigate('/interview/history')}>View All</Button>}
            style={cardStyle}
          >
            <Table
              dataSource={data.recentInterviews}
              columns={historyColumns}
              rowKey="id"
              pagination={false}
              size="small"
              style={{ background: 'transparent' }}
              rowClassName={() => 'dark-table-row'}
            />
          </Card>
        </Col>

        {/* Weak Areas */}
        <Col xs={24} lg={10}>
          <Card
            title={
              <Space>
                <FireOutlined style={{ color: '#EF4444' }} />
                <Text style={{ color: '#fff', fontWeight: 600 }}>Areas to Improve</Text>
              </Space>
            }
            style={cardStyle}
          >
            {data.weakAreas.length > 0 ? (
              <div>
                {data.weakAreas.map((area, i) => (
                  <div key={i} style={{ marginBottom: 16 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                      <Text style={{ color: '#fff', fontSize: 13 }}>{area.topic}</Text>
                      <Text style={{ color: 'rgba(255,255,255,0.4)', fontSize: 12 }}>
                        {area.occurrences} times
                      </Text>
                    </div>
                    <Progress
                      percent={Math.min(area.occurrences * 20, 100)}
                      strokeColor={{ from: '#EF4444', to: '#F59E0B' }}
                      trailColor="rgba(255,255,255,0.06)"
                      showInfo={false}
                      size="small"
                    />
                  </div>
                ))}
                <Tooltip title="Generate a personalized roadmap">
                  <Button
                    block
                    style={{
                      marginTop: 8,
                      background: 'rgba(108,99,255,0.1)',
                      border: '1px solid rgba(108,99,255,0.3)',
                      color: '#6C63FF',
                      borderRadius: 8,
                    }}
                    onClick={() => navigate('/roadmap')}
                  >
                    Generate Study Roadmap
                  </Button>
                </Tooltip>
              </div>
            ) : (
              <Empty description={<Text style={{ color: 'rgba(255,255,255,0.4)' }}>No data yet</Text>} />
            )}
          </Card>
        </Col>
      </Row>
    </div>
  );
}
