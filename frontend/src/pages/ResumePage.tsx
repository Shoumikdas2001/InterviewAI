import { useEffect, useState, useCallback } from 'react';
import {
  Card, Upload, Typography, Button, Progress, Tag, Row, Col,
  Space, Spin, Empty, Modal, message, Tooltip,
} from 'antd';
import {
  InboxOutlined, FileTextOutlined, DeleteOutlined,
  ThunderboltOutlined, DownloadOutlined, CheckCircleOutlined,
  LoadingOutlined,
} from '@ant-design/icons';
import { resumeApi } from '../api/resume.api';
import type { Resume } from '../types';

const { Title, Text, Paragraph } = Typography;
const { Dragger } = Upload;

function ScoreRing({ score, label, color }: { score: number; label: string; color: string }) {
  return (
    <div style={{ textAlign: 'center' }}>
      <Progress
        type="circle"
        percent={score}
        strokeColor={color}
        trailColor="rgba(255,255,255,0.06)"
        format={(p) => <Text style={{ color: '#fff', fontWeight: 700 }}>{p}%</Text>}
        size={80}
      />
      <Text style={{ color: 'rgba(255,255,255,0.6)', fontSize: 12, display: 'block', marginTop: 8 }}>
        {label}
      </Text>
    </div>
  );
}

export function ResumePage() {
  const [resumes, setResumes] = useState<Resume[]>([]);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [analyzing, setAnalyzing] = useState<string | null>(null);
  const [selectedResume, setSelectedResume] = useState<Resume | null>(null);

  const loadResumes = useCallback(() => {
    resumeApi.getAll()
      .then(setResumes)
      .catch(() => message.error('Failed to load resumes'))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { loadResumes(); }, [loadResumes]);

  const handleUpload = async (file: File) => {
    setUploading(true);
    try {
      await resumeApi.upload(file);
      message.success(`${file.name} uploaded successfully!`);
      loadResumes();
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message
        || 'Upload failed. Ensure the file is a PDF under 5MB.';
      message.error(msg);
    } finally {
      setUploading(false);
    }
    return false; // Prevent default upload behavior
  };

  const handleAnalyze = async (id: string) => {
    setAnalyzing(id);
    try {
      await resumeApi.analyze(id);
      message.success('ATS analysis complete!');
      const updated = await resumeApi.getById(id);
      setResumes((prev) => prev.map((r) => r.id === id ? updated : r));
      if (selectedResume?.id === id) setSelectedResume(updated);
    } catch {
      message.error('Analysis failed. Please try again.');
    } finally {
      setAnalyzing(null);
    }
  };

  const handleDelete = (id: string) => {
    Modal.confirm({
      title: 'Delete Resume?',
      content: 'This action cannot be undone.',
      okText: 'Delete',
      okButtonProps: { danger: true },
      onOk: async () => {
        await resumeApi.delete(id);
        setResumes((prev) => prev.filter((r) => r.id !== id));
        if (selectedResume?.id === id) setSelectedResume(null);
        message.success('Resume deleted.');
      },
    });
  };

  const handleDownload = async (id: string, filename: string) => {
    const blob = await resumeApi.download(id);
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  };

  const getAtsColor = (score: number) =>
    score >= 80 ? '#10B981' : score >= 60 ? '#F59E0B' : '#EF4444';

  return (
    <div>
      <div style={{ marginBottom: 32 }}>
        <Title level={2} style={{ color: '#fff', margin: 0 }}>
          <FileTextOutlined style={{ color: '#6C63FF', marginRight: 12 }} />
          Resume Management
        </Title>
        <Text style={{ color: 'rgba(255,255,255,0.5)' }}>
          Upload your resume to get an ATS score and personalize interview questions
        </Text>
      </div>

      <Row gutter={[24, 24]}>
        {/* Upload Panel */}
        <Col xs={24} lg={10}>
          <Card style={{
            background: 'rgba(255,255,255,0.02)',
            border: '1px solid rgba(255,255,255,0.07)',
            borderRadius: 20,
          }}>
            <Title level={4} style={{ color: '#fff', marginBottom: 16 }}>Upload Resume</Title>
            <Dragger
              id="resume-upload"
              accept=".pdf"
              showUploadList={false}
              beforeUpload={handleUpload}
              disabled={uploading}
              style={{
                background: 'rgba(108,99,255,0.05)',
                border: '2px dashed rgba(108,99,255,0.3)',
                borderRadius: 12,
              }}
            >
              <div style={{ padding: '32px 16px' }}>
                {uploading ? (
                  <LoadingOutlined style={{ fontSize: 40, color: '#6C63FF' }} />
                ) : (
                  <InboxOutlined style={{ fontSize: 48, color: '#6C63FF' }} />
                )}
                <Title level={5} style={{ color: '#fff', marginTop: 16 }}>
                  {uploading ? 'Uploading...' : 'Drop your PDF here'}
                </Title>
                <Paragraph style={{ color: 'rgba(255,255,255,0.5)', margin: 0, fontSize: 13 }}>
                  or click to select · PDF only · Max 5MB
                </Paragraph>
              </div>
            </Dragger>

            <div style={{ marginTop: 20 }}>
              <Title level={5} style={{ color: 'rgba(255,255,255,0.7)', marginBottom: 12 }}>
                💡 Tips for a High ATS Score
              </Title>
              {[
                'Use standard section headings (Experience, Skills, Education)',
                'Include keywords from the job description',
                'Avoid graphics, tables, and columns',
                'Use standard fonts (Arial, Calibri)',
                'Save as PDF (text-based, not scanned)',
              ].map((tip, i) => (
                <div key={i} style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
                  <CheckCircleOutlined style={{ color: '#10B981', marginTop: 2, flexShrink: 0 }} />
                  <Text style={{ color: 'rgba(255,255,255,0.6)', fontSize: 13 }}>{tip}</Text>
                </div>
              ))}
            </div>
          </Card>
        </Col>

        {/* Resume List */}
        <Col xs={24} lg={14}>
          {loading ? (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 300 }}>
              <Spin size="large" />
            </div>
          ) : resumes.length === 0 ? (
            <Card style={{
              background: 'rgba(255,255,255,0.02)',
              border: '1px solid rgba(255,255,255,0.07)',
              borderRadius: 20,
              textAlign: 'center',
              padding: '60px 32px',
            }}>
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description={<Text style={{ color: 'rgba(255,255,255,0.5)' }}>No resumes uploaded yet</Text>}
              />
            </Card>
          ) : (
            <div>
              {resumes.map((resume) => (
                <Card
                  key={resume.id}
                  onClick={() => setSelectedResume(selectedResume?.id === resume.id ? null : resume)}
                  style={{
                    background: selectedResume?.id === resume.id
                      ? 'rgba(108,99,255,0.08)'
                      : 'rgba(255,255,255,0.02)',
                    border: selectedResume?.id === resume.id
                      ? '1px solid rgba(108,99,255,0.4)'
                      : '1px solid rgba(255,255,255,0.07)',
                    borderRadius: 16,
                    cursor: 'pointer',
                    marginBottom: 16,
                    transition: 'all 0.2s',
                  }}
                >
                  <Row align="middle" gutter={16}>
                    <Col>
                      <FileTextOutlined style={{ fontSize: 28, color: '#6C63FF' }} />
                    </Col>
                    <Col flex="auto">
                      <Text style={{ color: '#fff', fontWeight: 600, display: 'block' }}>
                        {resume.fileName}
                      </Text>
                      <Text style={{ color: 'rgba(255,255,255,0.4)', fontSize: 12 }}>
                        {(resume.fileSizeBytes / 1024).toFixed(0)} KB ·{' '}
                        {new Date(resume.uploadedAt).toLocaleDateString()}
                      </Text>
                    </Col>
                    <Col>
                      {resume.atsAnalysis ? (
                        <Tooltip title="ATS Score">
                          <div style={{
                            width: 52, height: 52, borderRadius: '50%',
                            border: `2px solid ${getAtsColor(resume.atsAnalysis.atsScore)}`,
                            display: 'flex', flexDirection: 'column',
                            alignItems: 'center', justifyContent: 'center',
                          }}>
                            <Text style={{ color: getAtsColor(resume.atsAnalysis.atsScore), fontWeight: 700, fontSize: 13 }}>
                              {resume.atsAnalysis.atsScore}
                            </Text>
                            <Text style={{ color: 'rgba(255,255,255,0.4)', fontSize: 9 }}>ATS</Text>
                          </div>
                        </Tooltip>
                      ) : (
                        <Tag color="default">Not analyzed</Tag>
                      )}
                    </Col>
                    <Col>
                      <Space onClick={(e) => e.stopPropagation()}>
                        {!resume.atsAnalysis && (
                          <Button
                            type="primary"
                            icon={analyzing === resume.id ? <LoadingOutlined /> : <ThunderboltOutlined />}
                            size="small"
                            loading={analyzing === resume.id}
                            style={{ background: 'linear-gradient(135deg, #6C63FF, #48c6ef)', border: 'none' }}
                            onClick={() => handleAnalyze(resume.id)}
                          >
                            Analyze
                          </Button>
                        )}
                        <Tooltip title="Download">
                          <Button
                            icon={<DownloadOutlined />}
                            size="small"
                            type="text"
                            style={{ color: 'rgba(255,255,255,0.5)' }}
                            onClick={() => handleDownload(resume.id, resume.fileName)}
                          />
                        </Tooltip>
                        <Tooltip title="Delete">
                          <Button
                            icon={<DeleteOutlined />}
                            size="small"
                            type="text"
                            danger
                            onClick={() => handleDelete(resume.id)}
                          />
                        </Tooltip>
                      </Space>
                    </Col>
                  </Row>

                  {/* Expanded Analysis */}
                  {selectedResume?.id === resume.id && resume.atsAnalysis && (
                    <div style={{ marginTop: 20, borderTop: '1px solid rgba(255,255,255,0.07)', paddingTop: 20 }}>
                      <Row gutter={16} justify="space-around" style={{ marginBottom: 20 }}>
                        <Col><ScoreRing score={resume.atsAnalysis.atsScore} label="ATS Score" color="#6C63FF" /></Col>
                        <Col><ScoreRing score={resume.atsAnalysis.formattingScore} label="Formatting" color="#48c6ef" /></Col>
                        <Col><ScoreRing score={resume.atsAnalysis.keywordScore} label="Keywords" color="#10B981" /></Col>
                        <Col><ScoreRing score={resume.atsAnalysis.experienceScore} label="Experience" color="#F59E0B" /></Col>
                      </Row>

                      <Row gutter={16}>
                        <Col xs={24} md={12}>
                          <Title level={5} style={{ color: '#10B981', marginBottom: 8 }}>✓ Strengths</Title>
                          {resume.atsAnalysis.strengths.slice(0, 3).map((s, i) => (
                            <Text key={i} style={{ color: 'rgba(255,255,255,0.7)', display: 'block', fontSize: 13, marginBottom: 4 }}>
                              • {s}
                            </Text>
                          ))}
                        </Col>
                        <Col xs={24} md={12}>
                          <Title level={5} style={{ color: '#EF4444', marginBottom: 8 }}>✗ Weaknesses</Title>
                          {resume.atsAnalysis.weaknesses.slice(0, 3).map((w, i) => (
                            <Text key={i} style={{ color: 'rgba(255,255,255,0.7)', display: 'block', fontSize: 13, marginBottom: 4 }}>
                              • {w}
                            </Text>
                          ))}
                        </Col>
                      </Row>

                      {resume.atsAnalysis.missingKeywords.length > 0 && (
                        <div style={{ marginTop: 12 }}>
                          <Text style={{ color: 'rgba(255,255,255,0.5)', fontSize: 12 }}>Missing Keywords:</Text>
                          <div style={{ marginTop: 6 }}>
                            {resume.atsAnalysis.missingKeywords.slice(0, 8).map((kw, i) => (
                              <Tag key={i} color="orange" style={{ marginBottom: 4 }}>{kw}</Tag>
                            ))}
                          </div>
                        </div>
                      )}
                    </div>
                  )}
                </Card>
              ))}
            </div>
          )}
        </Col>
      </Row>
    </div>
  );
}
