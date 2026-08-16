import React from 'react';
import { CheckCircle2 } from 'lucide-react';
import './VotingOption.css';

interface VotingOptionProps {
  id: string;
  text: string;
  isSelected: boolean;
  onSelect: (id: string) => void;
  disabled?: boolean;
}

export const VotingOption: React.FC<VotingOptionProps> = ({ id, text, isSelected, onSelect, disabled }) => {
  return (
    <div 
      className={`vote-card ${isSelected ? 'active' : ''}`} 
      onClick={() => !disabled && onSelect(id)}
      role="radio"
      aria-checked={isSelected}
      style={{ opacity: disabled ? 0.7 : 1, cursor: disabled ? 'not-allowed' : 'pointer' }}
    >
      <div className="vote-card-content">
        <span className="vote-card-text">{text}</span>
        {isSelected && <CheckCircle2 className="vote-icon" size={20} />}
      </div>
    </div>
  );
};
