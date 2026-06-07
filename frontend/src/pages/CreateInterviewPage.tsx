import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Card, Form, Select, Button, Typography, Row, Col,
  Tag, Slider, message, Steps, Divider,
} from 'antd';
import {
  CodeOutlined, UserOutlined, ThunderboltOutlined,
  AppstoreOutlined, BulbOutlined, ArrowRightOutlined,
} from '@ant-design/icons';
import { interviewApi } from '../api/interview.api';
import type { InterviewType, DifficultyLevel } from '../types';
import { useEffect } from 'react';
import { resumeApi } from '../api/resume.api';
import type { Resume } from '../types';

const { Title, Text } = Typography;
const { Option } = Select;

const POPULAR_ROLES = [
  'Software Engineer', 'Frontend Developer', 'Backend Developer',
  'Full Stack Developer', 'Data Scientist', 'DevOps Engineer',
  'Machine Learning Engineer', 'Product Manager', 'QA Engineer',
];

const COMMON_SKILLS = [
  'JavaScript', 'TypeScript', 'React', 'Node.js', 'Python',
  'Java', 'C#', 'SQL', 'MongoDB', 'System Design', 'REST APIs',
  'Data Structures', 'Algorithms', 'Git', 'AWS', 'Docker',
  'Kubernetes', 'CI/CD', 'Agile', 'Problem Solving',
];

const INTERVIEW_TYPES: { key: InterviewType; label: string; icon: React.ReactNode; desc: string }[] = [
  { key: 'Technical', icon: <CodeOutlined />, label: 'Technical', desc: 'Coding, algorithms, system design' },
  { key: 'Behavioral', icon: <UserOutlined />, label: 'Behavioral', desc: 'STAR method, soft skills' },
  { key: 'Mixed', icon: <AppstoreOutlined />, label: 'Mixed', desc: 'Both technical and behavioral' },
  { key: 'SystemDesign', icon: <ThunderboltOutlined />, label: 'System Design', desc: 'Architecture, scalability' },
];

export function CreateInterviewPage() {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [selectedType, setSelectedType] = useState<InterviewType>('Technical');
  const [difficulty, setDifficulty] = useState<DifficultyLevel>('Medium');
  const [resumes, setResumes] = useState<Resume[]>([]);
  const [form] = Form.useForm();

  useEffect(() => {
    resumeApi.getAll().then(setResumes).catch(() => {});
  }, []);

  const handleSubmit = async (values: {
    jobRole: string;
    experienceLevel: string;
    skills: string[];
    resumeId?: string;
  }) => {
    setIsLoading(true);
    try {
      const result = await interviewApi.create({
        ...values,
        interviewType: selectedType,
        difficulty,
      });
      message.success('Interview created! Get ready...');
      navigate(`/interview/${result.session.id}/room`);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        || 'Failed to create interview';
      message.error(msg);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: 800, margin: '0 auto' }}>
      <div style={{ marginBottom: 32 }}>
        <Title level={2} style={{ color: '#fff', margin: 0 }}>
          <BulbOutlined style={{ color: '#6C63FF', marginRight: 12 }} />
          New Interview Session
        </Title>
        <Text style={{ color: 'rgba(255,255,255,0.5)' }}>
          Configure your AI-powered mock interview
        </Text>
      </div>

      <Steps
        current={0}
        items={[
          { title: <Text style={{ color: '#fff' }}>Configure</Text> },
          { title: <Text style={{ color: 'rgba(255,255,255,0.5)' }}>Interview</Text> },
          { title: <Text style={{ color: 'rgba(255,255,255,0.5)' }}>Results</Text> },
        ]}
        style={{ marginBottom: 32 }}
      />

      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        size="large"
        initialValues={{ experienceLevel: 'Mid-level', skills: [] }}
      >
        {/* Role & Experience */}
        <Card style={{
          background: 'rgba(255,255,255,0.02)',
          border: '1px solid rgba(255,255,255,0.07)',
          borderRadius: 16, marginBottom: 16,
        }}>
          <Title level={4} style={{ color: '#fff', marginBottom: 20 }}>
            Job Details
          </Title>
          <Row gutter={16}>
            <Col xs={24} md={14}>
              <Form.Item
                label={<Text style={{ color: 'rgba(255,255,255,0.7)' }}>Target Job Role</Text>}
                name="jobRole"
                rules={[{ required: true, message: 'Please enter the job role' }]}
              >
                <Select
                  id="job-role-select"
                  showSearch
                  mode={undefined}
                  placeholder="e.g. Software Engineer"
                  style={{ width: '100%' }}
                >
                  {POPULAR_ROLES.map((r) => <Option key={r} value={r}>{r}</Option>)}
                </Select>
              </Form.Item>
            </Col>
            <Col xs={24} md={10}>
              <Form.Item
                label={<Text style={{ color: 'rgba(255,255,255,0.7)' }}>Experience Level</Text>}
                name="experienceLevel"
              >
                <Select id="exp-level-select">
                  {['Junior (0-2 years)', 'Mid-level (2-5 years)', 'Senior (5-8 years)', 'Staff+ (8+ years)'].map((e) => (
                    <Option key={e} value={e}>{e}</Option>
                  ))}
                </Select>
              </Form.Item>
            </Col>
          </Row>

          <Form.Item
            label={<Text style={{ color: 'rgba(255,255,255,0.7)' }}>Skills to be Tested</Text>}
            name="skills"
            rules={[{ required: true, message: 'Select at least 2 skills', type: 'array', min: 2 }]}
          >
            <Select
              id="skills-select"
              mode="tags"
              placeholder="Select or type skills..."
              maxTagCount={8}
              allowClear
            >
              {COMMON_SKILLS.map((s) => <Option key={s} value={s}>{s}</Option>)}
            </Select>
          </Form.Item>

          {resumes.length > 0 && (
            <Form.Item
              label={<Text style={{ color: 'rgba(255,255,255,0.7)' }}>Link Resume (optional)</Text>}
              name="resumeId"
            >
              <Select id="resume-select" placeholder="Select a resume to personalize questions" allowClear>
                {resumes.map((r) => (
                  <Option key={r.id} value={r.id}>
                    {r.fileName} {r.atsAnalysis ? `(ATS: ${r.atsAnalysis.atsScore}%)` : ''}
                  </Option>
                ))}
              </Select>
            </Form.Item>
          )}
        </Card>

        {/* Interview Type */}
        <Card style={{
          background: 'rgba(255,255,255,0.02)',
          border: '1px solid rgba(255,255,255,0.07)',
          borderRadius: 16, marginBottom: 16,
        }}>
          <Title level={4} style={{ color: '#fff', marginBottom: 20 }}>
            Interview Type
          </Title>
          <Row gutter={12}>
            {INTERVIEW_TYPES.map((t) => (
              <Col xs={12} md={6} key={t.key}>
                <div
                  id={`interview-type-${t.key}`}
                  onClick={() => setSelectedType(t.key)}
                  style={{
                    padding: '16px 12px',
                    border: `2px solid ${selectedType === t.key ? '#6C63FF' : 'rgba(255,255,255,0.08)'}`,
                    borderRadius: 12,
                    background: selectedType === t.key ? 'rgba(108,99,255,0.1)' : 'rgba(255,255,255,0.02)',
                    cursor: 'pointer',
                    textAlign: 'center',
                    transition: 'all 0.2s',
                  }}
                >
                  <div style={{ fontSize: 24, color: selectedType === t.key ? '#6C63FF' : 'rgba(255,255,255,0.5)', marginBottom: 8 }}>
                    {t.icon}
                  </div>
                  <Text style={{ color: '#fff', fontWeight: 600, display: 'block', fontSize: 13 }}>
                    {t.label}
                  </Text>
                  <Text style={{ color: 'rgba(255,255,255,0.4)', fontSize: 11 }}>
                    {t.desc}
                  </Text>
                </div>
              </Col>
            ))}
          </Row>
        </Card>

        {/* Difficulty */}
        <Card style={{
          background: 'rgba(255,255,255,0.02)',
          border: '1px solid rgba(255,255,255,0.07)',
          borderRadius: 16, marginBottom: 24,
        }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 12 }}>
            <Title level={4} style={{ color: '#fff', margin: 0 }}>Difficulty</Title>
            <Tag color={difficulty === 'Easy' ? 'green' : difficulty === 'Hard' ? 'red' : 'orange'}>
              {difficulty}
            </Tag>
          </div>
          <Slider
            id="difficulty-slider"
            min={0} max={2}
            value={['Easy', 'Medium', 'Hard'].indexOf(difficulty)}
            onChange={(v) => setDifficulty(['Easy', 'Medium', 'Hard'][v] as DifficultyLevel)}
            marks={{ 0: <Text style={{ color: '#10B981' }}>Easy</Text>, 1: <Text style={{ color: '#F59E0B' }}>Medium</Text>, 2: <Text style={{ color: '#EF4444' }}>Hard</Text> }}
            step={1}
            tooltip={{ formatter: null }}
            styles={{ track: { background: 'linear-gradient(90deg, #10B981, #F59E0B, #EF4444)' } }}
          />
        </Card>

        <Divider style={{ borderColor: 'rgba(255,255,255,0.07)' }} />

        <Button
          id="create-interview-btn"
          type="primary"
          htmlType="submit"
          loading={isLoading}
          size="large"
          block
          icon={<ArrowRightOutlined />}
          style={{
            height: 54, fontSize: 16, fontWeight: 700,
            background: 'linear-gradient(135deg, #6C63FF, #48c6ef)',
            border: 'none', borderRadius: 12,
            boxShadow: '0 8px 32px rgba(108,99,255,0.4)',
          }}
        >
          {isLoading ? 'Generating Questions with AI...' : 'Start Interview →'}
        </Button>

        <Text style={{ display: 'block', textAlign: 'center', color: 'rgba(255,255,255,0.3)', marginTop: 12, fontSize: 12 }}>
          8 AI-generated questions tailored to your profile
        </Text>
      </Form>
    </div>
  );
}
