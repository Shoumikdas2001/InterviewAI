import { useEffect, useState } from 'react';
import {
  Card, Typography, Tag, Button, Progress, Space, Spin,
  Form, Select, Row, Col, message,
} from 'antd';
import {
  RocketOutlined, CalendarOutlined, ClockCircleOutlined,
  BulbOutlined, BookOutlined, CodeOutlined, TrophyOutlined,
} from '@ant-design/icons';
import { interviewApi } from '../api/interview.api';
import type { StudyPlan } from '../types';

const { Title, Text, Paragraph } = Typography;
const { Option } = Select;

const POPULAR_ROLES = [
  'Software Engineer', 'Frontend Developer', 'Backend Developer',
  'Full Stack Developer', 'Data Scientist', 'DevOps Engineer',
];

const COMMON_WEAK_AREAS = [
  'System Design', 'Algorithms', 'Data Structures', 'SQL', 'React',
  'TypeScript', 'Node.js', 'Docker', 'AWS', 'Testing', 'Performance',
  'Security', 'Databases', 'REST APIs', 'Git', 'Problem Solving',
];

const priorityColor = {
  High: '#EF4444',
  Medium: '#F59E0B',
  Low: '#10B981',
};

export function RoadmapPage() {
  const [roadmap, setRoadmap] = useState<StudyPlan | null>(null);
  const [loading, setLoading] = useState(true);
  const [generating, setGenerating] = useState(false);
  const [form] = Form.useForm();

  useEffect(() => {
    interviewApi.getLatestRoadmap()
      .then(setRoadmap)
      .finally(() => setLoading(false));
  }, []);

  const handleGenerate = async (values: { jobRole: string; weakAreas: string[] }) => {
    setGenerating(true);
    try {
      const plan = await interviewApi.generateRoadmap(values.jobRole, values.weakAreas);
      setRoadmap(plan);
      message.success('Your personalized roadmap is ready! 🚀');
    } catch {
      message.error('Failed to generate roadmap. Please try again.');
    } finally {
      setGenerating(false);
    }
  };

  const completedWeeks = 0; // Future: track progress in localStorage/backend

  return (
    <div>
      <div style={{ marginBottom: 32 }}>
        <Title level={2} style={{ color: '#fff', margin: '0 0 8px' }}>
          <RocketOutlined style={{ color: '#6C63FF', marginRight: 12 }} />
          Learning Roadmap
        </Title>
        <Text style={{ color: 'rgba(255,255,255,0.5)' }}>
          AI-generated 4-week study plan tailored to your weak areas
        </Text>
      </div>

      <Row gutter={[24, 24]}>
        {/* Generator Panel */}
        <Col xs={24} lg={8}>
          <Card style={{
            background: 'rgba(255,255,255,0.02)',
            border: '1px solid rgba(255,255,255,0.07)',
            borderRadius: 20,
            position: 'sticky',
            top: 80,
          }}>
            <Title level={4} style={{ color: '#fff', marginBottom: 20 }}>
              <BulbOutlined style={{ color: '#F59E0B', marginRight: 8 }} />
              Generate Roadmap
            </Title>

            <Form form={form} layout="vertical" onFinish={handleGenerate}>
              <Form.Item
                label={<Text style={{ color: 'rgba(255,255,255,0.7)' }}>Target Role</Text>}
                name="jobRole"
                rules={[{ required: true, message: 'Select a job role' }]}
              >
                <Select id="roadmap-role" placeholder="e.g. Software Engineer">
                  {POPULAR_ROLES.map((r) => <Option key={r} value={r}>{r}</Option>)}
                </Select>
              </Form.Item>

              <Form.Item
                label={<Text style={{ color: 'rgba(255,255,255,0.7)' }}>Weak Areas to Focus On</Text>}
                name="weakAreas"
                rules={[{ required: true, message: 'Select at least 1 area', type: 'array', min: 1 }]}
              >
                <Select
                  id="roadmap-weak-areas"
                  mode="multiple"
                  placeholder="Select topics to improve..."
                  maxTagCount={5}
                >
                  {COMMON_WEAK_AREAS.map((s) => <Option key={s} value={s}>{s}</Option>)}
                </Select>
              </Form.Item>

              <Button
                id="generate-roadmap-btn"
                type="primary"
                htmlType="submit"
                loading={generating}
                block
                icon={<RocketOutlined />}
                style={{
                  height: 44, fontWeight: 600,
                  background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
                  border: 'none', borderRadius: 10,
                }}
              >
                {generating ? 'Generating with AI...' : 'Generate Roadmap'}
              </Button>
            </Form>

            {roadmap && (
              <div style={{ marginTop: 24, padding: 16, background: 'rgba(108,99,255,0.08)', borderRadius: 12, border: '1px solid rgba(108,99,255,0.2)' }}>
                <Text style={{ color: 'rgba(255,255,255,0.7)', fontSize: 12, display: 'block', marginBottom: 8 }}>
                  Current Plan
                </Text>
                <Text style={{ color: '#fff', fontWeight: 600 }}>{roadmap.title}</Text>
                <div style={{ marginTop: 8 }}>
                  <Space size={4}>
                    <CalendarOutlined style={{ color: '#6C63FF' }} />
                    <Text style={{ color: 'rgba(255,255,255,0.5)', fontSize: 12 }}>
                      4 weeks · {roadmap.estimatedHours}h total
                    </Text>
                  </Space>
                </div>
                <Progress
                  percent={Math.round((completedWeeks / 4) * 100)}
                  strokeColor="linear-gradient(90deg, #6C63FF, #48c6ef)"
                  trailColor="rgba(255,255,255,0.06)"
                  format={() => `${completedWeeks}/4 weeks`}
                  style={{ marginTop: 8 }}
                  size="small"
                />
              </div>
            )}
          </Card>
        </Col>

        {/* Roadmap Content */}
        <Col xs={24} lg={16}>
          {loading ? (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 300 }}>
              <Spin size="large" />
            </div>
          ) : !roadmap ? (
            <Card style={{
              background: 'rgba(255,255,255,0.02)',
              border: '1px solid rgba(255,255,255,0.07)',
              borderRadius: 20,
              textAlign: 'center',
              padding: '60px 32px',
            }}>
              <div style={{ fontSize: 64, marginBottom: 16 }}>📚</div>
              <Title level={3} style={{ color: '#fff' }}>No Roadmap Yet</Title>
              <Paragraph style={{ color: 'rgba(255,255,255,0.5)' }}>
                Generate your personalized learning roadmap by selecting a target role
                and weak areas in the panel on the left.
              </Paragraph>
            </Card>
          ) : (
            <div>
              {/* Roadmap Header */}
              <Card style={{
                background: 'linear-gradient(135deg, rgba(108,99,255,0.15), rgba(72,198,239,0.08))',
                border: '1px solid rgba(108,99,255,0.2)',
                borderRadius: 20, marginBottom: 24,
              }}>
                <Row align="middle" gutter={16}>
                  <Col flex="auto">
                    <Title level={3} style={{ color: '#fff', margin: 0 }}>{roadmap.title}</Title>
                    <Space style={{ marginTop: 8 }}>
                      <Tag color="purple">{roadmap.jobRole}</Tag>
                      <Tag icon={<ClockCircleOutlined />}>{roadmap.estimatedHours}h estimated</Tag>
                      {roadmap.targetSkills.slice(0, 3).map((s, i) => (
                        <Tag key={i} color="blue">{s}</Tag>
                      ))}
                    </Space>
                  </Col>
                  <Col>
                    <TrophyOutlined style={{ fontSize: 48, color: 'rgba(108,99,255,0.4)' }} />
                  </Col>
                </Row>
              </Card>

              {/* Weeks */}
              {roadmap.weeks.map((week, weekIndex) => (
                <Card
                  key={weekIndex}
                  style={{
                    background: 'rgba(255,255,255,0.02)',
                    border: '1px solid rgba(255,255,255,0.07)',
                    borderRadius: 20, marginBottom: 16,
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 20 }}>
                    <div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 4 }}>
                        <div style={{
                          width: 36, height: 36, borderRadius: 10,
                          background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
                          display: 'flex', alignItems: 'center', justifyContent: 'center',
                          color: '#fff', fontWeight: 700,
                        }}>
                          {week.weekNumber}
                        </div>
                        <Title level={4} style={{ color: '#fff', margin: 0 }}>
                          Week {week.weekNumber}: {week.theme}
                        </Title>
                      </div>
                    </div>
                    <Tag icon={<ClockCircleOutlined />} color="default">
                      ~{week.estimatedHours}h
                    </Tag>
                  </div>

                  {/* Topics */}
                  <div style={{ marginBottom: 20 }}>
                    {week.topics.map((topic, ti) => (
                      <div
                        key={ti}
                        style={{
                          padding: '12px 16px',
                          background: 'rgba(255,255,255,0.03)',
                          border: '1px solid rgba(255,255,255,0.06)',
                          borderRadius: 10,
                          marginBottom: 8,
                          display: 'flex',
                          justifyContent: 'space-between',
                          alignItems: 'flex-start',
                        }}
                      >
                        <div>
                          <Text style={{ color: '#fff', fontWeight: 600, display: 'block' }}>{topic.name}</Text>
                          <Text style={{ color: 'rgba(255,255,255,0.5)', fontSize: 13 }}>{topic.description}</Text>
                        </div>
                        <Tag color={priorityColor[topic.priority as keyof typeof priorityColor]?.replace('#', '') || 'default'}
                          style={{ flexShrink: 0, marginLeft: 8 }}>
                          {topic.priority}
                        </Tag>
                      </div>
                    ))}
                  </div>

                  {/* Resources & Exercises */}
                  <Row gutter={16}>
                    <Col xs={24} md={12}>
                      <div>
                        <Text style={{ color: 'rgba(255,255,255,0.6)', fontSize: 12, fontWeight: 600 }}>
                          <BookOutlined style={{ marginRight: 6 }} />RESOURCES
                        </Text>
                        <ul style={{ margin: '8px 0', paddingLeft: 20 }}>
                          {week.resources.map((r, i) => (
                            <li key={i}>
                              <Text style={{ color: 'rgba(255,255,255,0.7)', fontSize: 13 }}>{r}</Text>
                            </li>
                          ))}
                        </ul>
                      </div>
                    </Col>
                    <Col xs={24} md={12}>
                      <div>
                        <Text style={{ color: 'rgba(255,255,255,0.6)', fontSize: 12, fontWeight: 600 }}>
                          <CodeOutlined style={{ marginRight: 6 }} />EXERCISES
                        </Text>
                        <ul style={{ margin: '8px 0', paddingLeft: 20 }}>
                          {week.exercises.map((e, i) => (
                            <li key={i}>
                              <Text style={{ color: 'rgba(255,255,255,0.7)', fontSize: 13 }}>{e}</Text>
                            </li>
                          ))}
                        </ul>
                      </div>
                    </Col>
                  </Row>
                </Card>
              ))}
            </div>
          )}
        </Col>
      </Row>
    </div>
  );
}
