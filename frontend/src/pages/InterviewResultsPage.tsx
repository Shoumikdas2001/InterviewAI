import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Card, Typography, Row, Col, Tag, Progress, Space, Button, Collapse,
  Spin, Tabs, List, Divider, message, Rate, Alert,
} from 'antd';
import {
  DownloadOutlined, RocketOutlined,
  CheckCircleOutlined, CloseCircleOutlined, BulbOutlined, ReloadOutlined,
} from '@ant-design/icons';
import {
  RadarChart, Radar, PolarGrid, PolarAngleAxis, PolarRadiusAxis,
  ResponsiveContainer, BarChart, Bar, XAxis, YAxis, Tooltip, CartesianGrid,
} from 'recharts';
import { interviewApi, reportsApi } from '../api/interview.api';
import type { InterviewSession, Question, Answer, SkillGapAnalysis } from '../types';

const { Title, Text, Paragraph } = Typography;

function ScoreBadge({ score }: { score: number }) {
  const color = score >= 7 ? '#10B981' : score >= 5 ? '#F59E0B' : '#EF4444';
  return (
    <div style={{
      width: 80, height: 80, borderRadius: '50%',
      border: `3px solid ${color}`,
      display: 'flex', flexDirection: 'column',
      alignItems: 'center', justifyContent: 'center',
    }}>
      <Text style={{ color, fontSize: 24, fontWeight: 800 }}>{score.toFixed(1)}</Text>
      <Text style={{ color: 'rgba(255,255,255,0.4)', fontSize: 10 }}>/10</Text>
    </div>
  );
}

export function InterviewResultsPage() {
  const { id: sessionId } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [session, setSession] = useState<InterviewSession | null>(null);
  const [questions, setQuestions] = useState<Question[]>([]);
  const [answers, setAnswers] = useState<Answer[]>([]);
  const [skillGap, setSkillGap] = useState<SkillGapAnalysis | null>(null);
  const [loading, setLoading] = useState(true);
  const [analyzing, setAnalyzing] = useState(false);
  const [generatingReport, setGeneratingReport] = useState(false);

  useEffect(() => {
    if (!sessionId) return;

    Promise.all([
      interviewApi.getSession(sessionId),
      interviewApi.getQuestions(sessionId),
      interviewApi.getAnswers(sessionId),
    ]).then(([s, qs, as]) => {
      setSession(s);
      setQuestions(qs);
      setAnswers(as);
      if (s.skillGapAnalysis) setSkillGap(s.skillGapAnalysis);
    }).catch(console.error)
      .finally(() => setLoading(false));
  }, [sessionId]);

  const handleGenerateSkillGap = async () => {
    setAnalyzing(true);
    try {
      const gap = await interviewApi.generateSkillGap(sessionId!);
      setSkillGap(gap);
      // Refresh session scores
      const updated = await interviewApi.getSession(sessionId!);
      setSession(updated);
      message.success('Skill gap analysis complete!');
    } catch {
      message.error('Analysis failed. Ensure all questions are answered.');
    } finally {
      setAnalyzing(false);
    }
  };

  const handleDownloadReport = async () => {
    setGeneratingReport(true);
    try {
      const blob = await reportsApi.generate(sessionId!);
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `InterviewAI-Report-${new Date().toISOString().slice(0, 10)}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
      message.success('Report downloaded!');
    } catch {
      message.error('Failed to generate report');
    } finally {
      setGeneratingReport(false);
    }
  };

  if (loading) return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 400 }}>
      <Spin size="large" />
    </div>
  );

  const overallScore = session?.overallScore ?? 0;
  const scoreColor = overallScore >= 7 ? '#10B981' : overallScore >= 5 ? '#F59E0B' : '#EF4444';

  const scoreBreakdown = [
    { name: 'Technical', score: (session?.technicalScore ?? 0) * 10 },
    { name: 'Communication', score: (session?.communicationScore ?? 0) * 10 },
    { name: 'Confidence', score: (session?.confidenceScore ?? 0) * 10 },
    { name: 'Problem Solving', score: (session?.problemSolvingScore ?? 0) * 10 },
  ];

  const radarData = skillGap?.skillScores
    ? Object.entries(skillGap.skillScores).slice(0, 7).map(([k, v]) => ({ skill: k, score: v }))
    : [];

  const qaItems = questions.map((q) => {
    const answer = answers.find((a) => a.questionId === q.id);
    return { question: q, answer };
  });

  return (
    <div>
      {/* Header */}
      <div style={{
        display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start',
        marginBottom: 32, flexWrap: 'wrap', gap: 16,
      }}>
        <div>
          <Title level={2} style={{ color: '#fff', margin: '0 0 4px' }}>
            Interview Results
          </Title>
          <Text style={{ color: 'rgba(255,255,255,0.5)' }}>
            {session?.title} · {session?.jobRole}
          </Text>
        </div>
        <Space wrap>
          <Button
            icon={<DownloadOutlined />}
            loading={generatingReport}
            onClick={handleDownloadReport}
            style={{ background: 'rgba(255,255,255,0.06)', border: '1px solid rgba(255,255,255,0.1)', color: '#fff' }}
          >
            Download PDF Report
          </Button>
          <Button
            type="primary"
            icon={<RocketOutlined />}
            style={{ background: 'linear-gradient(135deg, #6C63FF, #48c6ef)', border: 'none' }}
            onClick={() => navigate('/roadmap')}
          >
            View Roadmap
          </Button>
          <Button
            icon={<ReloadOutlined />}
            onClick={() => navigate('/interview/new')}
          >
            New Interview
          </Button>
        </Space>
      </div>

      {/* Score Hero */}
      <Card style={{
        background: `linear-gradient(135deg, rgba(${scoreColor === '#10B981' ? '16,185,129' : scoreColor === '#F59E0B' ? '245,158,11' : '239,68,68'},0.1) 0%, rgba(108,99,255,0.05) 100%)`,
        border: `1px solid ${scoreColor}30`,
        borderRadius: 20, marginBottom: 24,
      }}>
        <Row gutter={24} align="middle">
          <Col xs={24} md={8} style={{ textAlign: 'center' }}>
            <div style={{ fontSize: 72, fontWeight: 900, color: scoreColor, lineHeight: 1 }}>
              {overallScore.toFixed(1)}
            </div>
            <Text style={{ color: 'rgba(255,255,255,0.5)', fontSize: 18 }}>/10 Overall Score</Text>
            <div style={{ marginTop: 12 }}>
              <Rate
                disabled
                defaultValue={Math.round(overallScore / 2)}
                style={{ color: scoreColor }}
              />
            </div>
            <Tag
              color={overallScore >= 7 ? 'green' : overallScore >= 5 ? 'orange' : 'red'}
              style={{ marginTop: 8, fontSize: 13 }}
            >
              {overallScore >= 7 ? '🏆 Excellent' : overallScore >= 5 ? '📈 Good' : '📚 Needs Improvement'}
            </Tag>
          </Col>
          <Col xs={24} md={16}>
            <Row gutter={12}>
              {scoreBreakdown.map((item) => (
                <Col xs={12} key={item.name}>
                  <div style={{ marginBottom: 16 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                      <Text style={{ color: 'rgba(255,255,255,0.7)', fontSize: 13 }}>{item.name}</Text>
                      <Text style={{ color: '#fff', fontWeight: 600, fontSize: 13 }}>
                        {(item.score / 10).toFixed(1)}/10
                      </Text>
                    </div>
                    <Progress
                      percent={item.score}
                      strokeColor={{ from: '#6C63FF', to: '#48c6ef' }}
                      trailColor="rgba(255,255,255,0.06)"
                      showInfo={false}
                      size="small"
                    />
                  </div>
                </Col>
              ))}
            </Row>
          </Col>
        </Row>
      </Card>

      <Tabs
        defaultActiveKey="skillgap"
        items={[
          {
            key: 'skillgap',
            label: 'Skill Gap Analysis',
            children: skillGap ? (
              <div>
                <Row gutter={16} style={{ marginBottom: 24 }}>
                  <Col xs={24} lg={12}>
                    <Card style={{ background: 'rgba(255,255,255,0.02)', border: '1px solid rgba(255,255,255,0.07)', borderRadius: 16 }}>
                      <Title level={5} style={{ color: '#10B981' }}>
                        <CheckCircleOutlined /> Strengths
                      </Title>
                      <List
                        dataSource={skillGap.strengths}
                        renderItem={(item) => (
                          <List.Item style={{ borderColor: 'rgba(255,255,255,0.06)' }}>
                            <Text style={{ color: 'rgba(255,255,255,0.8)' }}>✓ {item}</Text>
                          </List.Item>
                        )}
                      />
                    </Card>
                  </Col>
                  <Col xs={24} lg={12}>
                    <Card style={{ background: 'rgba(255,255,255,0.02)', border: '1px solid rgba(255,255,255,0.07)', borderRadius: 16 }}>
                      <Title level={5} style={{ color: '#EF4444' }}>
                        <CloseCircleOutlined /> Weaknesses
                      </Title>
                      <List
                        dataSource={skillGap.weaknesses}
                        renderItem={(item) => (
                          <List.Item style={{ borderColor: 'rgba(255,255,255,0.06)' }}>
                            <Text style={{ color: 'rgba(255,255,255,0.8)' }}>✗ {item}</Text>
                          </List.Item>
                        )}
                      />
                    </Card>
                  </Col>
                </Row>

                {radarData.length > 0 && (
                  <Card style={{ background: 'rgba(255,255,255,0.02)', border: '1px solid rgba(255,255,255,0.07)', borderRadius: 16, marginBottom: 16 }}>
                    <Title level={5} style={{ color: '#fff' }}>Skill Scores</Title>
                    <Row gutter={16}>
                      <Col xs={24} md={12}>
                        <ResponsiveContainer width="100%" height={220}>
                          <RadarChart data={radarData}>
                            <PolarGrid stroke="rgba(255,255,255,0.1)" />
                            <PolarAngleAxis dataKey="skill" tick={{ fill: 'rgba(255,255,255,0.6)', fontSize: 11 }} />
                            <PolarRadiusAxis domain={[0, 100]} tick={{ fill: 'rgba(255,255,255,0.3)', fontSize: 9 }} />
                            <Radar dataKey="score" stroke="#6C63FF" fill="#6C63FF" fillOpacity={0.3} />
                          </RadarChart>
                        </ResponsiveContainer>
                      </Col>
                      <Col xs={24} md={12}>
                        <ResponsiveContainer width="100%" height={220}>
                          <BarChart data={radarData} layout="vertical">
                            <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.05)" />
                            <XAxis type="number" domain={[0, 100]} tick={{ fill: 'rgba(255,255,255,0.4)', fontSize: 10 }} />
                            <YAxis type="category" dataKey="skill" tick={{ fill: 'rgba(255,255,255,0.6)', fontSize: 10 }} width={90} />
                            <Tooltip contentStyle={{ background: '#1a1650', border: 'none', borderRadius: 8 }} />
                            <Bar dataKey="score" fill="url(#barGrad)" radius={[0, 4, 4, 0]} />
                            <defs>
                              <linearGradient id="barGrad" x1="0" y1="0" x2="1" y2="0">
                                <stop offset="0%" stopColor="#6C63FF" />
                                <stop offset="100%" stopColor="#48c6ef" />
                              </linearGradient>
                            </defs>
                          </BarChart>
                        </ResponsiveContainer>
                      </Col>
                    </Row>
                  </Card>
                )}

                <Card style={{ background: 'rgba(255,255,255,0.02)', border: '1px solid rgba(255,255,255,0.07)', borderRadius: 16 }}>
                  <Title level={5} style={{ color: '#F59E0B' }}>
                    <BulbOutlined /> Recommendations
                  </Title>
                  <List
                    dataSource={skillGap.recommendations}
                    renderItem={(item, i) => (
                      <List.Item style={{ borderColor: 'rgba(255,255,255,0.06)' }}>
                        <Text style={{ color: 'rgba(255,255,255,0.8)' }}>
                          <Text style={{ color: '#6C63FF', fontWeight: 700 }}>{i + 1}.</Text> {item}
                        </Text>
                      </List.Item>
                    )}
                  />
                  <Divider style={{ borderColor: 'rgba(255,255,255,0.07)' }} />
                  <Button
                    type="primary"
                    icon={<RocketOutlined />}
                    style={{ background: 'linear-gradient(135deg, #6C63FF, #48c6ef)', border: 'none', borderRadius: 8 }}
                    onClick={() => navigate('/roadmap')}
                  >
                    Generate Personalized Roadmap
                  </Button>
                </Card>
              </div>
            ) : (
              <Card style={{ background: 'rgba(255,255,255,0.02)', border: '1px solid rgba(255,255,255,0.07)', borderRadius: 16, textAlign: 'center', padding: '48px 32px' }}>
                <Alert
                  message="Generate Skill Gap Analysis"
                  description="Get a comprehensive analysis of your strengths, weaknesses, and personalized recommendations."
                  type="info"
                  style={{ background: 'rgba(108,99,255,0.1)', border: '1px solid rgba(108,99,255,0.3)', marginBottom: 24, borderRadius: 12 }}
                />
                <Button
                  id="generate-skill-gap-btn"
                  type="primary"
                  size="large"
                  loading={analyzing}
                  style={{ background: 'linear-gradient(135deg, #6C63FF, #48c6ef)', border: 'none', borderRadius: 10 }}
                  onClick={handleGenerateSkillGap}
                >
                  {analyzing ? 'Analyzing with AI...' : 'Generate Skill Gap Analysis'}
                </Button>
              </Card>
            ),
          },
          {
            key: 'qa',
            label: `Q&A Review (${qaItems.length})`,
            children: (
              <div>
                {qaItems.map(({ question, answer }, i) => (
                  <Card
                    key={question.id}
                    style={{
                      background: 'rgba(255,255,255,0.02)',
                      border: '1px solid rgba(255,255,255,0.07)',
                      borderRadius: 16, marginBottom: 16,
                    }}
                  >
                    <Row gutter={16} align="middle" style={{ marginBottom: 12 }}>
                      <Col>
                        <Tag color="purple">Q{i + 1}</Tag>
                        <Tag>{question.category}</Tag>
                        <Tag color="blue">{question.skillTag}</Tag>
                      </Col>
                      {answer?.evaluation && (
                        <Col flex="auto" style={{ textAlign: 'right' }}>
                          <ScoreBadge score={answer.evaluation.overallScore} />
                        </Col>
                      )}
                    </Row>

                    <Paragraph style={{ color: '#fff', fontSize: 16, fontWeight: 600 }}>
                      {question.questionText}
                    </Paragraph>

                    {answer && (
                      <>
                        <Divider style={{ borderColor: 'rgba(255,255,255,0.07)' }} />
                        <Text style={{ color: 'rgba(255,255,255,0.5)', fontSize: 12 }}>Your Answer:</Text>
                        <Paragraph style={{ color: 'rgba(255,255,255,0.8)', background: 'rgba(255,255,255,0.03)', padding: 12, borderRadius: 8, marginTop: 8 }}>
                          {answer.answerText}
                        </Paragraph>

                        {answer.evaluation && (
                          <Collapse
                            ghost
                            items={[{
                              key: '1',
                              label: <Text style={{ color: '#6C63FF' }}>AI Feedback</Text>,
                              children: (
                                <div>
                                  <Row gutter={8} style={{ marginBottom: 12 }}>
                                    {[
                                      { label: 'Technical', score: answer.evaluation.technicalScore },
                                      { label: 'Clarity', score: answer.evaluation.clarityScore },
                                      { label: 'Completeness', score: answer.evaluation.completenessScore },
                                      { label: 'Confidence', score: answer.evaluation.confidenceScore },
                                    ].map((dim) => (
                                      <Col span={6} key={dim.label}>
                                        <Text style={{ color: 'rgba(255,255,255,0.4)', fontSize: 10, display: 'block' }}>{dim.label}</Text>
                                        <Text style={{
                                          color: dim.score >= 7 ? '#10B981' : dim.score >= 5 ? '#F59E0B' : '#EF4444',
                                          fontWeight: 700,
                                        }}>{dim.score}/10</Text>
                                      </Col>
                                    ))}
                                  </Row>

                                  {answer.evaluation.feedback.map((f, fi) => (
                                    <Paragraph key={fi} style={{ color: 'rgba(255,255,255,0.7)', margin: '4px 0', fontSize: 13 }}>
                                      • {f}
                                    </Paragraph>
                                  ))}

                                  {answer.evaluation.improvementAreas.length > 0 && (
                                    <Alert
                                      message="Improve:"
                                      description={answer.evaluation.improvementAreas.join(' · ')}
                                      type="warning"
                                      style={{ background: 'rgba(245,158,11,0.1)', border: '1px solid rgba(245,158,11,0.2)', marginTop: 8, borderRadius: 8 }}
                                    />
                                  )}
                                </div>
                              ),
                            }]}
                          />
                        )}
                      </>
                    )}
                  </Card>
                ))}
              </div>
            ),
          },
        ]}
      />
    </div>
  );
}
